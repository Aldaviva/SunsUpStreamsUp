using Microsoft.Extensions.Options;
using NodaTime;
using OBSStudioClient.Enums;
using OBSStudioClient.Exceptions;
using SolCalc;
using SolCalc.Data;
using SunsUpStreamsUp.Options;
using ThrottleDebounce;
using Twitch.Net.Models;
using Twitch.Net.Models.Responses;
using Unfucked;
using Unfucked.DateTime;
using Unfucked.OBS;
using Unfucked.Twitch;

namespace SunsUpStreamsUp.Logic;

public class StreamManager(
    SolarEventEmitter solarEventEmitter,
    IObsClientFactory obsClientFactory,
    ITwitchApi? twitch,
    IClock clock,
    ILogger<StreamManager> logger,
    IOptions<StreamOptions> options
): IHostedService, IDisposable {

    private IObsClient? obs;

    public async Task StartAsync(CancellationToken cancellationToken) {
        try {
            Uri websocketServerUrl = new UrlBuilder("ws", options.Value.obsHostname, options.Value.obsPort);
            logger.LogDebug("Connecting to OBS at {url}", websocketServerUrl);

            try {
                const int          MAX_ATTEMPTS = 34;
                ObsFailedToConnect exception    = new(options.Value.obsHostname, options.Value.obsPort, !string.IsNullOrEmpty(options.Value.obsPassword));
                obs = await Retrier.Attempt(async _ => {
                        IObsClient? iObsClient = await obsClientFactory.Connect(websocketServerUrl, options.Value.obsPassword, cancellationToken);
                        return iObsClient ?? throw exception;
                    },
                    maxAttempts: MAX_ATTEMPTS,
                    delay: Retrier.Delays.Linear((Seconds) 1, (Seconds) 1, (Seconds) 10), // 4m55s
                    isRetryAllowed: _ => true,
                    beforeRetry: (i, _) => {
                        logger.LogWarning("Failed to connect to OBS, retrying #{attempt:N0}/{max:N0}", i + 1, MAX_ATTEMPTS - 1);
                        return Task.CompletedTask;
                    },
                    cancellationToken: cancellationToken);
            } catch (TaskCanceledException) {
                return;
            } catch (Exception e) when (e is not OutOfMemoryException) {
                logger.LogError(
                    """
                    {type}: {message}

                    Make sure:
                    1) OBS is running
                    2) the OBS WebSocket server is enabled in Tools > WebSocket Server Settings
                    3) {hostname} is the correct hostname of the computer running OBS
                    4) {port} is the correct TCP port of the OBS WebSocket server (default 4455)
                    5) inbound connections to TCP port {port} on the OBS computer are not being blocked by a firewall, if this program is running on a different computer from OBS
                    6) the OBS WebSocket password is set correctly in appsettings.json
                    7) the password is not URL-encoded (e.g. you must use ' ' instead of '%20' for a space), despite what the OBS WebSocket Connect Info dialog box shows; just normal JSON string escaping is required (e.g. '\"' instead of '"' for a double quotation mark)

                    Configure this program in {configFile}

                    """, e.GetType().Name, e.Message, options.Value.obsHostname, options.Value.obsPort, options.Value.obsPort,
                    Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory, "appsettings.json"));
                throw;
            }

            bool          isLocalObsStreamingNow = (await obs.GetStreamStatus()).OutputActive;
            Task<bool>    isRemotelyStreamingNow = isChannelLive();
            SunlightLevel currentSunlight        = solarEventEmitter.currentSunlight;
            bool          shouldBeStreamingNow   = currentSunlight >= solarEventEmitter.minimumSunlightLevel;

            logger.LogInformation("The sunlight level is {light}", currentSunlight.ToString(true));

            if (shouldBeStreamingNow && !isLocalObsStreamingNow && !await isRemotelyStreamingNow) {
                logger.LogInformation("Starting stream now because it should have already been live before this program launched, since it is currently {light}", currentSunlight.ToString(true));
                await obs.StartStream();
            } else if (isLocalObsStreamingNow || await isRemotelyStreamingNow) {
                logger.LogInformation("Stream is already live, leaving it running until {timeOfDay}", solarEventEmitter.minimumSunlightLevel.GetEnd(false).ToString(true));
            } else {
                logger.LogInformation("Stream is offline, not starting it until {timeOfDay}", solarEventEmitter.minimumSunlightLevel.GetStart(true).ToString(true));
            }

            solarEventEmitter.waitingForSolarElevationChange += onWaitForSolarElevationChange;
            solarEventEmitter.solarElevationChanged          += onSolarElevationChange;
        } catch (TaskCanceledException) { }
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        if (obs != null) {
            obs.Disconnect();
            logger.LogInformation("Disconnected from OBS");
        }
        return Task.CompletedTask;
    }

    private void onWaitForSolarElevationChange(object? sender, SunlightChange e) => logger.LogInformation(@"Waiting until {solarTime} to {action} the stream at {time:h:mm tt}, in {delay:h\h\ mm\m}",
        e.Name.ToString(true), e.IsSunRising ? "start" : "stop", e.Time, e.Time.ToInstant() - clock.GetCurrentInstant());

    private async void onSolarElevationChange(object? sender, SunlightChange e) {
        if (obs == null) return;

        bool shouldStreamBeLive = e.IsSunRising;
        logger.LogInformation("{action} stream because it is now {timeOfDay}", shouldStreamBeLive ? "Starting" : "Stopping", e.Name.ToString(true));

        if (!shouldStreamBeLive) {
            try {
                await obs.StopStream();
            } catch (ObsResponseException ex) when (ex.ErrorCode == RequestStatusCode.OutputNotRunning) {
                logger.LogDebug("Tried to stop stream but it was already stopped, doing nothing");
            }
        } else if (!await isChannelLive()) {
            await obs.StartStream();
        } else {
            logger.LogInformation("While attempting to start the stream, it was already live on another computer, so not interrupting the existing stream.");
        }
    }

    private async Task<bool> isChannelLive() {
        string? twitchUsername = options.Value.twitchUsername;
        if (twitch != null && !options.Value.replaceExistingStream && twitchUsername.HasText()) {
            logger.LogTrace("Checking if Twitch channel {username} is currently broadcasting from another computer", twitchUsername);
            HelixPaginatedResponse<HelixStream> userStreams;
            try {
                userStreams = await twitch.Streams.GetStreamsWithUserLogins([twitchUsername], 1);
            } catch (HttpRequestException e) {
                logger.LogWarning(e, "Failed to get stream state of Twitch user {username}, assuming stream is offline", twitchUsername);
                return false;
            }

            bool isLive = userStreams.Data.Length != 0;
            logger.LogTrace("Twitch channel {channel} is currently {isLive}broadcasting from another computer", twitchUsername, isLive ? "" : "not ");
            return isLive;
        } else {
            logger.LogTrace("No Twitch stream configured, so assuming the channel is not broadcasting from another computer");
            return false;
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            solarEventEmitter.waitingForSolarElevationChange -= onWaitForSolarElevationChange;
            solarEventEmitter.solarElevationChanged          -= onSolarElevationChange;
            obs?.Dispose();
            obs = null;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}