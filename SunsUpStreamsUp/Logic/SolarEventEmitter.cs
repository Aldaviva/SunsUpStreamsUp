using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Extensions;
using SunsUpStreamsUp.Math;
using SunsUpStreamsUp.Options;

namespace SunsUpStreamsUp.Logic;

public interface SolarEventEmitter: IHostedService, IDisposable {

    event EventHandler<SolarElevationChange> solarElevationChanged;

    Sunlight currentSunlight { get; }

}

public readonly record struct SolarElevationChange(
    ZonedDateTime time,
    Sunlight      oldSunlight,
    Sunlight      newSunlight,
    bool          isSunRising
);

public class SolarEventEmitterImpl(
    IClock                         unzonedClock,
    TimeProvider                   timeProvider,
    IOptions<GeographicOptions>    options,
    ILogger<SolarEventEmitterImpl> logger
): BackgroundService, SolarEventEmitter {

    private readonly ZonedClock clock = unzonedClock.InZone((options.Value.timeZone is { } id ? DateTimeZoneProviders.Tzdb.GetZoneOrNull(id) : null) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault());

    public event EventHandler<SolarElevationChange>? solarElevationChanged;

    public Sunlight currentSunlight => SunlightMath.getSunlightForTime(clock.GetCurrentZonedDateTime(), options.Value.latitude, options.Value.longitude);

    protected override async Task ExecuteAsync(CancellationToken cts) {
        ZonedDateTime now              = clock.GetCurrentZonedDateTime();
        Sunlight      previousSunlight = SunlightMath.getSunlightForTime(now, options.Value.latitude, options.Value.longitude);

        try {
            foreach (SunlightChanged brightnessChange in getSunlightChangesForUnlimitedDays(now, cts)) {
                cts.ThrowIfCancellationRequested();

                Duration delay = brightnessChange.startTime - clock.GetCurrentZonedDateTime();
                logger.LogDebug(@"Waiting {delay:h\h\ mm\m}, when {brightness} will start at {time:h:mm tt}", delay, brightnessChange.sunlightLevel.ToString(true), brightnessChange.startTime);
                await LongDelay(delay, timeProvider, cts);

                solarElevationChanged?.Invoke(this, new SolarElevationChange(brightnessChange.startTime, previousSunlight, brightnessChange.sunlightLevel, brightnessChange.isSunRising));

                previousSunlight = brightnessChange.sunlightLevel;
            }
        } catch (TaskCanceledException) { } catch (OperationCanceledException) { }
    }

    private IEnumerable<SunlightChanged> getSunlightChangesForUnlimitedDays(ZonedDateTime start, CancellationToken cts) {
        LocalDate                    startDate              = start.Date;
        decimal?                     previousSolarElevation = null;
        IEnumerable<SunlightChanged> sunlightChanges        = getSunlightChangesForSingleDay().Where(c => c.startTime.ToInstant() > start.ToInstant());

        while (!cts.IsCancellationRequested) {
            foreach (SunlightChanged brightnessChange in sunlightChanges) {
                cts.ThrowIfCancellationRequested();
                yield return brightnessChange;
                previousSolarElevation = brightnessChange.solarElevation;
            }

            startDate       = startDate.PlusDays(1);
            sunlightChanges = getSunlightChangesForSingleDay();
        }

        IEnumerable<SunlightChanged> getSunlightChangesForSingleDay() =>
            SunlightMath.getDailySunlightChanges(startDate, start.Zone, options.Value.latitude, options.Value.longitude, previousSolarElevation);
    }

    private static Task LongDelay(TimeSpan duration, TimeProvider? timeProvider = default, CancellationToken cancellationToken = default) {
        timeProvider ??= TimeProvider.System;

        // max duration of Task.Delay with .NET 6 and later
        TimeSpan                maxShortDelay = TimeSpan.FromMilliseconds(uint.MaxValue - 1);
        CancellationTokenSource testCts       = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try {
            Task.Delay(maxShortDelay, timeProvider, testCts.Token);
            testCts.Cancel();
        } catch (ArgumentOutOfRangeException) {
            // .NET 5 and earlier
            maxShortDelay = TimeSpan.FromMilliseconds(int.MaxValue);
        }

        Task result = Task.CompletedTask;

        for (TimeSpan remaining = duration; remaining > TimeSpan.Zero; remaining = remaining.Subtract(maxShortDelay)) {
            TimeSpan shortDelay = remaining > maxShortDelay ? maxShortDelay : remaining;
            result = result.ContinueWith(_ => Task.Delay(shortDelay, timeProvider, cancellationToken), cancellationToken,
                TaskContinuationOptions.LongRunning | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current).Unwrap();
        }

        return result;
    }

    private static Task LongDelay(Duration duration, TimeProvider? timeProvider = default, CancellationToken cancellationToken = default) {
        return LongDelay(duration.ToTimeSpan(), timeProvider, cancellationToken);
    }

}