using Microsoft.Extensions.Options;
using NodaTime;
using OBSStudioClient.Enums;
using OBSStudioClient.Exceptions;
using SolCalc;
using SolCalc.Data;
using SunsUpStreamsUp.Options;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ThrottleDebounce.Retry;
using Unfucked.DateTime;
using Unfucked.OBS;

namespace SunsUpStreamsUp.Logic;

public interface StreamManager: IDisposable, INotifyPropertyChanged {

    bool isLive { get; }

}

public class StreamManagerImpl(
    SolarEventEmitter solarEventEmitter,
    IObsClientFactory obsClientFactory,
    IClock clock,
    ILogger<StreamManagerImpl> logger,
    IOptions<StreamOptions> options
): IHostedService, StreamManager {

    private IObsClient? obs;

    public bool isLive {
        get;
        private set => setField(ref field, value);
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        try {
            Uri websocketServerUrl = new UrlBuilder("ws", options.Value.obsHostname, options.Value.obsPort);
            logger.Debug("Connecting to OBS at {url}", websocketServerUrl);

            try {
                ObsFailedToConnect exception = new(options.Value.obsHostname, options.Value.obsPort, !string.IsNullOrEmpty(options.Value.obsPassword));
                obs = await Retrier.Attempt(async _ => {
                    IObsClient? iObsClient = await obsClientFactory.Connect(websocketServerUrl, options.Value.obsPassword, cancellationToken);
                    return iObsClient ?? throw exception;
                }, new RetryOptions {
                    MaxOverallDuration = (Minutes) 5,
                    Delay              = Delays.Linear((Seconds) 1, (Seconds) 1, (Seconds) 10),
                    AfterFailure       = (e, _) => logger.Warn("Failed to connect to OBS: {msg}", e.MessageChain()),
                    BeforeRetry        = (_, i) => logger.Warn("Retrying #{attempt:N0}", i + 1),
                    IsRetryAllowed     = (e, _) => e is ObsFailedToConnect,
                    CancellationToken  = cancellationToken
                });
            } catch (TaskCanceledException) {
                return;
            } catch (Exception e) when (e is not OutOfMemoryException) {
                logger.Error(
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
            SunlightLevel currentSunlight        = solarEventEmitter.currentSunlight;
            bool          shouldBeStreamingNow   = currentSunlight >= solarEventEmitter.minimumSunlightLevel;

            logger.Info("The sunlight level is {light}", currentSunlight.ToString(true));

            if (shouldBeStreamingNow && !isLocalObsStreamingNow) {
                logger.Info("Starting stream now because it should have already been live before this program launched, since it is currently {light}", currentSunlight.ToString(true));
                await obs.StartStream();
            } else if (isLocalObsStreamingNow) {
                logger.Info("Stream is already live, leaving it running until {timeOfDay}", solarEventEmitter.minimumSunlightLevel.GetEnd(false).ToString(true));
                isLive = true;
            } else {
                logger.Info("Stream is offline, not starting it until {timeOfDay}", solarEventEmitter.minimumSunlightLevel.GetStart(true).ToString(true));
            }

            solarEventEmitter.waitingForSolarElevationChange += onWaitForSolarElevationChange;
            solarEventEmitter.solarElevationChanged          += onSolarElevationChange;
        } catch (TaskCanceledException) {}
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        if (obs != null) {
            obs.Disconnect();
            logger.Info("Disconnected from OBS");
        }
        return Task.CompletedTask;
    }

    private void onWaitForSolarElevationChange(object? sender, SunlightChange e) => logger.Info(@"Waiting until {solarTime} to {action} the stream at {time:h:mm tt}, in {delay:h\h\ mm\m}",
        e.Name.ToString(true), e.IsSunRising ? "start" : "stop", e.Time, e.Time.ToInstant() - clock.GetCurrentInstant());

    private async void onSolarElevationChange(object? sender, SunlightChange e) {
        if (obs == null) return;

        bool shouldStreamBeLive = e.IsSunRising;
        logger.Info("{action} stream because it is now {timeOfDay}", shouldStreamBeLive ? "Starting" : "Stopping", e.Name.ToString(true));

        if (!shouldStreamBeLive) {
            try {
                await obs.StopStream();
                isLive = false;
            } catch (ObsResponseException ex) when (ex.ErrorCode == RequestStatusCode.OutputNotRunning) {
                logger.Debug("Tried to stop stream but it was already stopped, doing nothing");
            }
        } else {
            await obs.StartStream();
            isLive = true;
        }
    }

    protected virtual void dispose(bool disposing) {
        if (disposing) {
            solarEventEmitter.waitingForSolarElevationChange -= onWaitForSolarElevationChange;
            solarEventEmitter.solarElevationChanged          -= onSolarElevationChange;
            obs?.Dispose();
            obs = null;
        }
    }

    public void Dispose() {
        dispose(true);
        GC.SuppressFinalize(this);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void onPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool setField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        onPropertyChanged(propertyName);
        return true;
    }

}