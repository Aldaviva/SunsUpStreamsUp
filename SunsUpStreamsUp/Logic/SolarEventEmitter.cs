using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Extensions;
using SolCalc;
using SolCalc.Data;
using SunsUpStreamsUp.Options;

namespace SunsUpStreamsUp.Logic;

public interface SolarEventEmitter: IHostedService, IDisposable {

    event EventHandler<SunlightChange> solarElevationChanged;
    event EventHandler<SunlightChange> waitingForSolarElevationChange;

    SunlightLevel currentSunlight { get; }
    SunlightLevel minimumSunlightLevel { get; }

}

public class SolarEventEmitterImpl(
    IClock unzonedClock,
    TimeProvider timeProvider,
    IOptions<GeographicOptions> options,
    ILogger<SolarEventEmitterImpl> logger
): BackgroundService, SolarEventEmitter {

    private readonly ZonedClock clock = unzonedClock.InZone((options.Value.timeZone is {} id ? DateTimeZoneProviders.Tzdb.GetZoneOrNull(id) : null) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault());

    public event EventHandler<SunlightChange>? solarElevationChanged;
    public event EventHandler<SunlightChange>? waitingForSolarElevationChange;

    public SunlightLevel currentSunlight => SunlightCalculator.GetSunlightAt(clock.GetCurrentZonedDateTime(), options.Value.latitude, options.Value.longitude);

    public SunlightLevel minimumSunlightLevel => options.Value.minimumSunlightLevel ?? SunlightLevel.CivilTwilight;

    protected override async Task ExecuteAsync(CancellationToken cts) {
        try {
            IEnumerable<SunlightChange> sunlightChanges = SunlightCalculator.GetSunlightChanges(clock.GetCurrentZonedDateTime(), options.Value.latitude, options.Value.longitude)
                .Where(change => (change.IsSunRising && change.NewSunlightLevel == minimumSunlightLevel) || (!change.IsSunRising && change.PreviousSunlightLevel == minimumSunlightLevel));

            foreach (SunlightChange sunlightChange in sunlightChanges) {
                cts.ThrowIfCancellationRequested();

                Duration delay = sunlightChange.Time - clock.GetCurrentZonedDateTime();
                logger.Debug(@"Waiting {delay:h\h\ mm\m}, when {brightness} will start at {time:h:mm tt}", delay, sunlightChange.NewSunlightLevel.ToString(true), sunlightChange.Time);
                waitingForSolarElevationChange?.Invoke(this, sunlightChange);
                await Tasks.Delay(delay.ToTimeSpan(), timeProvider, cts);

                solarElevationChanged?.Invoke(this, sunlightChange);
            }
        } catch (TaskCanceledException) {} catch (OperationCanceledException) {}
    }

}