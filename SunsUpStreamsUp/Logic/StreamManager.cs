using Microsoft.Extensions.Options;
using OBSStudioClient.Enums;
using SunsUpStreamsUp.Facades;
using SunsUpStreamsUp.Math;
using SunsUpStreamsUp.Options;
using System.Net.WebSockets;

namespace SunsUpStreamsUp.Logic;

public class StreamManager(
    SolarEventEmitter       solarEventEmitter,
    IObsClient              obs,
    IOptions<StreamOptions> options,
    ILogger<StreamManager>  logger
): BackgroundService {

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        try {
            await connectToObs(cancellationToken);

            bool     isStreamingNow       = (await obs.GetStreamStatus()).OutputActive;
            Sunlight currentSunlight      = solarEventEmitter.currentSunlight;
            bool     shouldBeStreamingNow = currentSunlight is Sunlight.DAYLIGHT or Sunlight.CIVIL_TWILIGHT;

            logger.LogInformation("Current solar light level: {light}", currentSunlight.ToString(true));

            if (shouldBeStreamingNow && !isStreamingNow) {
                logger.LogInformation("Starting stream now because it should have already been started before this program launched, since it is currently {light}",
                    currentSunlight.ToString(true));
                await obs.StartStream();
            } else if (isStreamingNow) {
                logger.LogInformation("Stream is live now, waiting until the end of civil twilight to stop it");
            } else {
                logger.LogInformation("Stream is not live now, waiting until the beginning of civil twilight to start it");
            }

            solarEventEmitter.solarElevationChanged += onSolarElevationChange;
        } catch (TaskCanceledException) { }
    }

    private async void onSolarElevationChange(object? sender, SolarElevationChange e) {
        switch (e) {
            case { isSunRising: true, oldSunlight: Sunlight.NAUTICAL_TWILIGHT, newSunlight: Sunlight.CIVIL_TWILIGHT }:
                // Start stream at beginning of civil dawn
                logger.LogInformation("Starting stream now because civil twilight began");
                await obs.StartStream();
                break;
            case { isSunRising: false, oldSunlight: Sunlight.CIVIL_TWILIGHT, newSunlight: Sunlight.NAUTICAL_TWILIGHT }:
                // Stop stream at end of civil dusk
                logger.LogInformation("Stopping stream now because civil twilight ended");
                await obs.StopStream();
                break;
            default:
                logger.LogDebug("Ignoring solar elevation change event: isSunRising={isSunRising}, oldSunlight={oldSunlight}, newSunlight={newSunlight}, time={time}", e.isSunRising, e.oldSunlight,
                    e.newSunlight, e.time);
                break;
        }
    }

    private async Task connectToObs(CancellationToken cancellationToken = default) {
        TaskCompletionSource authenticated = new();
        obs.PropertyChanged += (_, eventArgs) => {
            if (eventArgs.PropertyName == nameof(IObsClient.ConnectionState) && obs.ConnectionState == ConnectionState.Connected) {
                authenticated.SetResult();
            }
        };

        logger.LogDebug("Connecting to OBS at ws://{host}:{port}", options.Value.obsHostname, options.Value.obsPort);
        if (!await obs.ConnectAsync(true, options.Value.obsPassword, options.Value.obsHostname, options.Value.obsPort, EventSubscriptions.None)) {
            throw new WebSocketException($"Failed to connect to OBS at {options.Value.obsHostname}:{options.Value.obsPort}");
        }

        await authenticated.Task.WaitAsync(cancellationToken);
        logger.LogInformation("Connected to OBS");
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            solarEventEmitter.solarElevationChanged -= onSolarElevationChange;
        }
    }

    public sealed override void Dispose() {
        Dispose(true);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

}