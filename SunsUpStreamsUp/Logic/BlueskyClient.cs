using Microsoft.Extensions.Options;
using NodaTime;
using SunsUpStreamsUp.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using ThrottleDebounce.Retry;
using Unfucked.DateTime;
using Unfucked.HTTP;
using Unfucked.HTTP.Exceptions;
using Timer = System.Timers.Timer;

namespace SunsUpStreamsUp.Logic;

public class BlueskyClient: IHostedService, IDisposable {

    private static readonly Duration MAX_STATUS_DURATION = (Hours) 4;

    private readonly IWebTarget              blueskyTarget;
    private readonly BlueskyAuthFilter       authFilter;
    private readonly IOptions<SocialOptions> options;
    private readonly ILogger<BlueskyClient>  logger;
    private readonly bool                    isEnabled;
    private readonly AsyncRetryOptions       alreadyLiveRetryOptions;
    private readonly Timer                   reapplyStatusTimer = new(MAX_STATUS_DURATION.Minus((Minutes) 5).ToTimeSpan()) { AutoReset = true, Enabled = false };

    public BlueskyClient(StreamManager streamManager, IHttpClient http, BlueskyAuthFilter authFilter, IOptions<SocialOptions> options, ILogger<BlueskyClient> logger) {
        this.authFilter = authFilter;
        this.options    = options;
        this.logger     = logger;

        isEnabled = options.Value.blueskyUsername.HasText() && options.Value.blueskyPassword.HasText() && options.Value.twitchUsername.HasText();

        blueskyTarget = http.Target("https://bsky.social/xrpc/").Register(authFilter);

        alreadyLiveRetryOptions = new AsyncRetryOptions {
            MaxAttempts    = 2,
            IsRetryAllowed = async (e, _) => e is BadRequestException,
            AfterFailure   = async (_, _) => await goDead()
        };

        streamManager.PropertyChanged += async (_, e) => {
            if (e.PropertyName == nameof(StreamManager.isLive)) {
                await setUserStream(streamManager.isLive);
            }
        };

        reapplyStatusTimer.Elapsed += async (_, _) => {
            if (streamManager.isLive) {
                await goDead();
                await goLive();
            }
        };
    }

    private async Task setUserStream(bool isLive) {
        if (!isEnabled) return;

        await authFilter.ensureUserIdExists(); // userId is needed for request bodies constructed before the auth filter would normally run

        if (isLive) {
            await goLive();
            reapplyStatusTimer.Enabled = true;
        } else {
            reapplyStatusTimer.Enabled = false;
            await goDead();
        }
    }

    private async Task goLive() {
        JsonObject body = (JsonObject) JsonNode.Parse(START_STREAM_BODY_TEMPLATE)!;
        body["repo"]                                    = JsonValue.Create(authFilter.userId);
        body["record"]!["createdAt"]                    = JsonValue.Create(DateTime.UtcNow);
        body["record"]!["durationMinutes"]              = JsonValue.Create((int) MAX_STATUS_DURATION.TotalMinutes);
        body["record"]!["embed"]!["external"]!["title"] = JsonValue.Create($"{options.Value.twitchUsername} on Twitch");
        body["record"]!["embed"]!["external"]!["uri"]   = JsonValue.Create(new UrlBuilder("https", "twitch.tv").Path(options.Value.twitchUsername!.ToLowerInvariant()).ToString());

        try {
            await Retrier.Attempt(async _ => await blueskyTarget.Path("com.atproto.repo.putRecord").Post<string>(JsonContent.Create(body)), alreadyLiveRetryOptions);
            logger.Debug("Live status added to Bluesky");
        } catch (WebApplicationException e) {
            logger.Error("Failed to go live on Bluesky: {status} {err}", e.StatusCode, Encoding.UTF8.GetString(e.ResponseBody!.Value.Span));
            throw;
        }
    }

    private async Task goDead() {
        using HttpResponseMessage response = await blueskyTarget
            .Path("com.atproto.repo.deleteRecord")
            .Post(JsonContent.Create(new {
                collection = "app.bsky.actor.status",
                repo       = authFilter.userId,
                rkey       = "self"
            }));

        if (response.IsSuccessStatusCode) {
            logger.Debug("Live status deleted from Bluesky");
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose() {
        reapplyStatusTimer.Dispose();
        GC.SuppressFinalize(this);
    }

    // language=json
    private const string START_STREAM_BODY_TEMPLATE = """
        {
            "repo": null,
            "collection": "app.bsky.actor.status",
            "rkey": "self",
            "record": {
                "$type": "app.bsky.actor.status",
                "createdAt": null,
                "status": "app.bsky.actor.status#live",
                "durationMinutes": null,
                "embed": {
                    "$type": "app.bsky.embed.external",
                    "external": {
                        "$type": "app.bsky.embed.external#external",
                        "title": "Twitch",
                        "description": "Twitch is the world",
                        "uri": null
                    }
                }
            },
            "swapRecord": null
        }
        """;

}