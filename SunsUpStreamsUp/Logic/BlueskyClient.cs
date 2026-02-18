using Microsoft.Extensions.Options;
using NodaTime;
using SunsUpStreamsUp.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using ThrottleDebounce.Retry;
using Unfucked.DateTime;
using Unfucked.HTTP;
using Unfucked.HTTP.Config;
using Unfucked.HTTP.Exceptions;
using Unfucked.HTTP.Filters;
using Timer = System.Timers.Timer;

namespace SunsUpStreamsUp.Logic;

public interface BlueskyClient: IDisposable {

    Task setUserStream(bool isLive);

}

public class BlueskyClientImpl: IHostedService, ClientRequestFilter, BlueskyClient {

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

    private static readonly PropertyKey<bool> IS_AUTH_REQUEST     = new($"{nameof(BlueskyClientImpl)}.{nameof(IS_AUTH_REQUEST)}");
    private static readonly Duration          MAX_STATUS_DURATION = (Hours) 4;

    private readonly IWebTarget                 blueskyTarget;
    private readonly IOptions<SocialOptions>    options;
    private readonly ILogger<BlueskyClientImpl> logger;
    private readonly bool                       isEnabled;
    private readonly AsyncRetryOptions          alreadyLiveRetryOptions;
    private readonly Timer                      refreshStatusTimer = new(MAX_STATUS_DURATION.Minus((Minutes) 5).ToTimeSpan()) { AutoReset = true, Enabled = false };

    private string? cachedAuthToken;
    private string? userId;

    public BlueskyClientImpl(IHttpClient http, StreamManager streamManager, IOptions<SocialOptions> options, ILogger<BlueskyClientImpl> logger) {
        this.options = options;
        this.logger  = logger;

        isEnabled = options.Value.blueskyUsername.HasText() && options.Value.blueskyPassword.HasText() && options.Value.twitchUsername.HasText();

        blueskyTarget = http.Target("https://bsky.social/xrpc/").Register(this);

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

        refreshStatusTimer.Elapsed += async (_, _) => {
            if (streamManager.isLive) {
                await goDead();
                await goLive();
            }
        };
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task setUserStream(bool isLive) {
        if (!isEnabled) return;

        if (userId == null) {
            await signIn();
        }

        if (isLive) {
            await goLive();
            refreshStatusTimer.Enabled = true;
        } else {
            refreshStatusTimer.Enabled = false;
            await goDead();
        }
    }

    private async Task goLive() {
        JsonObject body = (JsonObject) JsonNode.Parse(START_STREAM_BODY_TEMPLATE)!;
        body["repo"]                                  = JsonValue.Create(userId);
        body["record"]!["createdAt"]                  = JsonValue.Create(DateTime.UtcNow);
        body["record"]!["durationMinutes"]            = JsonValue.Create((int) MAX_STATUS_DURATION.TotalMinutes);
        body["record"]!["embed"]!["external"]!["uri"] = JsonValue.Create(new UrlBuilder("https", "twitch.tv").Path(options.Value.twitchUsername).ToString());

        try {
            await Retrier.Attempt(async _ => await blueskyTarget.Path("com.atproto.repo.putRecord").Post<string>(JsonContent.Create(body)), alreadyLiveRetryOptions);
            logger.Debug("Live status added to Bluesky");
        } catch (WebApplicationException e) {
            logger.Error("Failed to go live on Bluesky: {status} {err}", e.StatusCode, Encoding.UTF8.GetString(e.ResponseBody!.Value.Span));
            throw;
        }
    }

    private async Task goDead() {
        (await blueskyTarget
            .Path("com.atproto.repo.deleteRecord")
            .Post(JsonContent.Create(new {
                collection = "app.bsky.actor.status",
                repo       = userId,
                rkey       = "self"
            }))).Dispose();
        logger.Debug("Live status deleted from Bluesky");
    }

    async ValueTask<HttpRequestMessage> ClientRequestFilter.Filter(HttpRequestMessage request, FilterContext context, CancellationToken ct) {
        if (cachedAuthToken is null
            && (!(context.Configuration?.Property(IS_AUTH_REQUEST, out bool isAuthRequest) ?? false) || !isAuthRequest)
            && options.Value.blueskyUsername.HasLength() && options.Value.blueskyPassword.HasLength()) {

            try {
                await signIn(ct);
            } catch (NotFoundException) {
                logger.Error("Failed to log into Bluesky as {user}", options.Value.blueskyUsername);
                return request;
            }
        }

        if (cachedAuthToken is not null && request.Headers.Authorization is null) {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cachedAuthToken);
        }

        return request;
    }

    private async Task signIn(CancellationToken ct = default) {
        JsonObject authResponse = await blueskyTarget
            .Path("com.atproto.server.createSession")
            .Property(IS_AUTH_REQUEST, true)
            .Post<JsonObject>(JsonContent.Create(new {
                identifier = options.Value.blueskyUsername,
                password   = options.Value.blueskyPassword
            }), ct);

        cachedAuthToken = authResponse["accessJwt"]!.GetValue<string>();
        userId          = authResponse["did"]!.GetValue<string>();
    }

    public void Dispose() {
        cachedAuthToken = null;
        refreshStatusTimer.Dispose();
        GC.SuppressFinalize(this);
    }

}