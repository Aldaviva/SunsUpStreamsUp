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
    IClock                         unzonedClock,
    TimeProvider                   timeProvider,
    IOptions<GeographicOptions>    options,
    ILogger<SolarEventEmitterImpl> logger
): BackgroundService, SolarEventEmitter {

    private static readonly TimeSpan MAX_SHORT_DELAY;

    private readonly ZonedClock clock = unzonedClock.InZone((options.Value.timeZone is { } id ? DateTimeZoneProviders.Tzdb.GetZoneOrNull(id) : null) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault());

    public event EventHandler<SunlightChange>? solarElevationChanged;
    public event EventHandler<SunlightChange>? waitingForSolarElevationChange;

    public SunlightLevel currentSunlight => SunlightCalculator.GetSunlightAt(clock.GetCurrentZonedDateTime(), options.Value.latitude, options.Value.longitude);

    public SunlightLevel minimumSunlightLevel => options.Value.minimumSunlightLevel ?? SunlightLevel.CivilTwilight;

    static SolarEventEmitterImpl() {
        // max duration of Task.Delay with .NET 6 and later
        MAX_SHORT_DELAY = TimeSpan.FromMilliseconds(uint.MaxValue - 1);
        try {
            CancellationTokenSource testCts = new();
            Task.Delay(MAX_SHORT_DELAY, testCts.Token);
            testCts.Cancel();
        } catch (ArgumentOutOfRangeException) {
            // max duration of Task.Delay with .NET 5 and earlier
            MAX_SHORT_DELAY = TimeSpan.FromMilliseconds(int.MaxValue);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken cts) {
        try {
            IEnumerable<SunlightChange> sunlightChanges = SunlightCalculator.GetSunlightChanges(clock.GetCurrentZonedDateTime(), options.Value.latitude, options.Value.longitude)
                .Where(change => (change.IsSunRising && change.NewSunlightLevel == minimumSunlightLevel) || (!change.IsSunRising && change.PreviousSunlightLevel == minimumSunlightLevel));

            foreach (SunlightChange sunlightChange in sunlightChanges) {
                cts.ThrowIfCancellationRequested();

                Duration delay = sunlightChange.Time - clock.GetCurrentZonedDateTime();
                logger.LogDebug(@"Waiting {delay:h\h\ mm\m}, when {brightness} will start at {time:h:mm tt}", delay, sunlightChange.NewSunlightLevel.ToString(true), sunlightChange.Time);
                waitingForSolarElevationChange?.Invoke(this, sunlightChange);
                await LongDelay(delay, timeProvider, cts);

                solarElevationChanged?.Invoke(this, sunlightChange);
            }
        } catch (TaskCanceledException) { } catch (OperationCanceledException) { }
    }

    private static Task LongDelay(TimeSpan duration, TimeProvider? timeProvider = default, CancellationToken cancellationToken = default) {
        timeProvider ??= TimeProvider.System;
        Task result = Task.CompletedTask;

        for (TimeSpan remaining = duration; remaining > TimeSpan.Zero; remaining = remaining.Subtract(MAX_SHORT_DELAY)) {
            TimeSpan shortDelay = remaining > MAX_SHORT_DELAY ? MAX_SHORT_DELAY : remaining;
            result = result.ContinueWith(_ => Task.Delay(shortDelay, timeProvider, cancellationToken), cancellationToken,
                TaskContinuationOptions.LongRunning | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current).Unwrap();
        }

        return result;
    }

    private static Task LongDelay(Duration duration, TimeProvider? timeProvider = default, CancellationToken cancellationToken = default) {
        return LongDelay(duration.ToTimeSpan(), timeProvider, cancellationToken);
    }

}