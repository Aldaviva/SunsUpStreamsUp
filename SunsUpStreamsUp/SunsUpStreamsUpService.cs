using Innovative.SolarCalculator;
using Microsoft.Extensions.Options;
using SunsUpStreamsUp.Facades;

namespace SunsUpStreamsUp;

public class SunsUpStreamsUpService(
    IObsClient                      obs,
    TimeProvider                    timeProvider,
    IOptions<Options>               options,
    ILogger<SunsUpStreamsUpService> logger
): BackgroundService {

    private readonly TimeZoneInfo timeZone = string.IsNullOrWhiteSpace(options.Value.timeZone) ? TimeZoneInfo.Local : TimeZoneInfo.FindSystemTimeZoneById(options.Value.timeZone);

    protected override async Task ExecuteAsync(CancellationToken cts) {

        SolarStreamAction nextStreamAction = getNextStreamAction(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone));

        bool wasStreamActiveBeforeStartup = (await obs.GetStreamStatus()).OutputActive;
        logger.LogInformation("Stream is {runningState}", wasStreamActiveBeforeStartup ? "running" : "stopped");
        if (!nextStreamAction.shouldStartStream && !wasStreamActiveBeforeStartup) {
            logger.LogInformation("Starting stream now because it should have already been started, but it was stopped when this program launched");
            await startOrStopStream(true);
        }

        try {
            while (!cts.IsCancellationRequested) {
                TimeSpan delay = nextStreamAction.time - timeProvider.GetLocalNow();

                logger.LogInformation(@"Waiting {delay:h\h\ mm\m} to {action} the stream at {time:h:mm tt}", delay, nextStreamAction.shouldStartStream ? "start" : "stop",
                    nextStreamAction.time);
                await LongDelay(delay, timeProvider, cts);

                logger.LogInformation("{action} stream now", nextStreamAction.shouldStartStream ? "Starting" : "Stopping");
                await startOrStopStream(nextStreamAction.shouldStartStream);

                nextStreamAction = getNextStreamAction(nextStreamAction.time);
            }
        } catch (TaskCanceledException) { }
    }

    internal SolarStreamAction getNextStreamAction(DateTimeOffset start) {
        // SolarTimes are unzoned and relative to the local time at the geographic coordinates, not the computer's local time zone
        SolarTimes     solarTimes        = new(start, options.Value.latitude, options.Value.longitude);
        DateTimeOffset dawn              = solarTimes.DawnCivil.withZone(timeZone);
        DateTimeOffset dusk              = solarTimes.DuskCivil.withZone(timeZone);
        bool           dayHasDawnAndDusk = dawn != dusk && (dawn.AddDays(1) - dusk).abs() > TimeSpan.FromSeconds(1);
        if (dayHasDawnAndDusk && dusk.Date > dawn.Date) {
            dusk = new DateTime(dawn.dateOnly(), dusk.timeOnly()).withZone(timeZone);
        }

        if (dayHasDawnAndDusk) {    // not polar day or night
            if (dawn < dusk) {      // regular day
                if (start < dawn) { // before sunrise
                    return new SolarStreamAction(true, dawn);
                } else if (start < dusk) { // during day
                    return new SolarStreamAction(false, dusk);
                }
            } else {                // midnight sun
                if (start < dusk) { // before early sunset
                    return new SolarStreamAction(false, dusk);
                } else if (start < dawn) { // during early darkness
                    return new SolarStreamAction(true, dawn);
                }
            }
        }

        return getNextStreamAction(start.AddDays(1).toStartOfDay(timeZone));
    }

    internal Task startOrStopStream(bool startStream) => startStream ? obs.StartStream() : obs.StopStream();

    private static Task LongDelay(TimeSpan duration, TimeProvider? timeProvider = default, CancellationToken cancellationToken = default) {
        timeProvider ??= TimeProvider.System;

        /*
         * max duration of Task.Delay starting with .NET 6
         * https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.delay?view=net-8.0#system-threading-tasks-task-delay(system-timespan-system-timeprovider-system-threading-cancellationtoken)
         */
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

}