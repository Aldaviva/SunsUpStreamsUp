using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using NodaTime;
using NodaTime.Extensions;
using SunsUpStreamsUp.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Unfucked.DateTime;
using Unfucked.HTTP;
using Unfucked.HTTP.Config;
using Unfucked.HTTP.Exceptions;
using Unfucked.HTTP.Filters;
using HttpHeaders = Unfucked.HTTP.HttpHeaders;

namespace SunsUpStreamsUp.Logic;

public class BlueskyAuthFilter(IHttpClient http, IOptions<SocialOptions> options, IClock clock, ILogger<BlueskyAuthFilter> logger): ClientRequestFilter, ClientResponseFilter {

    private static readonly PropertyKey<bool> IS_AUTH_REQUEST = new($"{nameof(BlueskyAuthFilter)}.{nameof(IS_AUTH_REQUEST)}");
    private static readonly Duration          EARLY_REFRESH   = (Minutes) 5;

    private readonly IWebTarget webTarget = http.Target("https://bsky.social/xrpc/");

    public string? userId { get; private set; }
    private string?  accessToken;
    private string?  refreshToken;
    private Instant? accessTokenExpiration;
    private string? username => options.Value.blueskyUsername;
    private string? password => options.Value.blueskyPassword;

    public async Task<string> ensureUserIdExists(CancellationToken cancellationToken = default) {
        if (userId is null) {
            await signIn(cancellationToken);
        }
        return userId!;
    }

    private async Task signIn(CancellationToken ct = default) {
        JsonObject authResponse = await webTarget
            .Path("com.atproto.server.createSession")
            .Property(IS_AUTH_REQUEST, true)
            .Post<JsonObject>(JsonContent.Create(new { identifier = username, password }), ct);

        handleAuthResponse(authResponse);
    }

    private async Task refresh(CancellationToken ct = default) {
        JsonObject authResponse = await webTarget
            .Path("com.atproto.server.refreshSession")
            .Property(IS_AUTH_REQUEST, true)
            .Header(HttpHeaders.AUTHORIZATION, "Bearer " + refreshToken)
            .Post<JsonObject>(null, ct);

        handleAuthResponse(authResponse);
    }

    public async ValueTask<HttpRequestMessage> Filter(HttpRequestMessage request, FilterContext context, CancellationToken ct) {
        if (!canAuthenticate(context)) return request;

        if (accessToken is null) {
            try {
                await signIn(ct);
            } catch (ClientErrorException) {
                logger.Error("Failed to log into Bluesky as {user}", username);
                return request;
            }
        } else if (refreshToken is not null && accessTokenExpiration < clock.GetCurrentInstant().Plus(EARLY_REFRESH)) {
            await refresh(ct);
        }

        if (accessToken is not null && request.Headers.Authorization is null) {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return request;
    }

    public async ValueTask<HttpResponseMessage> Filter(HttpResponseMessage originalResponse, FilterContext context, CancellationToken ct) {
        if (originalResponse.StatusCode == HttpStatusCode.BadRequest
            && refreshToken is not null
            && canAuthenticate(context)
            && originalResponse.RequestMessage is { RequestUri: not null } originalRequest
            && (await originalResponse.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct))?["error"]?.GetValue<string>() == "ExpiredToken") {

            await refresh(ct);

            originalRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpRequest replayedRequest = await HttpRequest.Copy(originalRequest);

            HttpResponseMessage replayedResponse = await http.SendAsync(replayedRequest, ct);
            return replayedResponse;
        } else {
            return originalResponse;
        }
    }

    private bool canAuthenticate(FilterContext context) =>
        username.HasLength() && password.HasLength() && (!(context.Configuration?.Property(IS_AUTH_REQUEST, out bool isAuthRequest) ?? false) || !isAuthRequest);

    private void handleAuthResponse(JsonObject authResponse) {
        accessToken           = authResponse["accessJwt"]!.GetValue<string>();
        accessTokenExpiration = new JsonWebToken(accessToken).ValidTo.ToInstant(); // 2 hours
        refreshToken          = authResponse["refreshJwt"]!.GetValue<string>();
        userId                = authResponse["did"]!.GetValue<string>();
    }

}