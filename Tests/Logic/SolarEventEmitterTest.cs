using FluentAssertions.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NodaTime.Extensions;
using SunsUpStreamsUp.Options;

namespace Tests.Logic;

public class SolarEventEmitterTest {

    private static readonly DateTimeZone      LOS_ANGELES = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
    private static readonly DateTimeZone      BERLIN      = DateTimeZoneProviders.Tzdb["Europe/Berlin"];
    private static readonly GeographicOptions OPTIONS     = new() { latitude = 37.35, longitude = -121.95, timeZone = LOS_ANGELES.Id };

    private          SolarEventEmitterImpl   solarEventEmitter = null!;
    private readonly IClock                  clock             = A.Fake<IClock>();
    private readonly TimeProvider            timeProvider      = A.Fake<TimeProvider>(fakeOptions => fakeOptions.Wrapping(TimeProvider.System));
    private readonly CancellationTokenSource cts               = new();

    public SolarEventEmitterTest() {
        init();
    }

    private void init() {
        solarEventEmitter?.Dispose();
        solarEventEmitter = new SolarEventEmitterImpl(clock, timeProvider, new OptionsWrapper<GeographicOptions>(OPTIONS), new NullLogger<SolarEventEmitterImpl>());
    }

    [Theory]
    [MemberData(nameof(getCurrentSunlightData))]
    public void getCurrentSunlight(ZonedDateTime now, Sunlight expected) {
        A.CallTo(() => clock.GetCurrentInstant()).Returns(now.ToInstant());

        Sunlight actual = solarEventEmitter.currentSunlight;
        actual.Should().Be(expected);
    }

    public static TheoryData<ZonedDateTime, Sunlight> getCurrentSunlightData => new() {
        { new LocalDateTime(2024, 1, 23, 2, 53).InZoneStrictly(LOS_ANGELES), Sunlight.NIGHT },
        { new LocalDateTime(2024, 1, 23, 6, 1).InZoneStrictly(LOS_ANGELES), Sunlight.ASTRONOMICAL_TWILIGHT },
        { new LocalDateTime(2024, 1, 23, 6, 33).InZoneStrictly(LOS_ANGELES), Sunlight.NAUTICAL_TWILIGHT },
        { new LocalDateTime(2024, 1, 23, 7, 3).InZoneStrictly(LOS_ANGELES), Sunlight.CIVIL_TWILIGHT },
        { new LocalDateTime(2024, 1, 23, 12, 19).InZoneStrictly(LOS_ANGELES), Sunlight.DAYLIGHT },
        { new LocalDateTime(2024, 1, 23, 12 + 5, 36).InZoneStrictly(LOS_ANGELES), Sunlight.CIVIL_TWILIGHT },
        { new LocalDateTime(2024, 1, 23, 12 + 6, 5).InZoneStrictly(LOS_ANGELES), Sunlight.NAUTICAL_TWILIGHT },
        { new LocalDateTime(2024, 1, 23, 12 + 6, 36).InZoneStrictly(LOS_ANGELES), Sunlight.ASTRONOMICAL_TWILIGHT },
        { new LocalDateTime(2024, 1, 23, 12 + 9, 30).InZoneStrictly(LOS_ANGELES), Sunlight.NIGHT },
    };

    [Fact]
    public async Task fireCivilDuskEvent() {
        // 1 minute before civil twilight ends and nautical twilight begins
        ZonedDateTime lastMinuteOfCivilDusk = new LocalDateTime(2024, 1, 23, 12 + 5, 50, 0).InZoneStrictly(LOS_ANGELES);
        SunlightMath.getSunlightForTime(lastMinuteOfCivilDusk, OPTIONS.latitude, OPTIONS.longitude).Should().Be(Sunlight.CIVIL_TWILIGHT, "precondition");

        A.CallTo(() => clock.GetCurrentInstant()).Returns(lastMinuteOfCivilDusk.ToInstant());

        // Make Task.Delay complete very quickly to speed up testing, instead of waiting 60 seconds
        A.CallTo(() => timeProvider.CreateTimer(A<TimerCallback>._, An<object?>._, A<TimeSpan>._, A<TimeSpan>._))
            .ReturnsLazily<ITimer, TimerCallback, object?, TimeSpan, TimeSpan>((timerCallback, state, dueTime, period) =>
                TimeProvider.System.CreateTimer(timerCallback, state, TimeSpan.FromMilliseconds(1), period));

        TaskCompletionSource<SolarElevationChange> eventFired = new();
        solarEventEmitter.solarElevationChanged += (_, change) => { eventFired.SetResult(change); };

        Task executeTask = solarEventEmitter.StartAsync(cts.Token);

        SolarElevationChange actual = await eventFired.Task.WaitAsync(TimeSpan.FromSeconds(5), cts.Token);

        actual.isSunRising.Should().BeFalse();
        actual.time.Should().Be(new LocalDateTime(2024, 1, 23, 12 + 5, 51).InZoneStrictly(LOS_ANGELES));
        actual.oldSunlight.Should().Be(Sunlight.CIVIL_TWILIGHT, "civil twilight is ending");
        actual.newSunlight.Should().Be(Sunlight.NAUTICAL_TWILIGHT, "nautical twilight is beginning");

        await cts.CancelAsync();
        await executeTask;

        A.CallTo(() => timeProvider.CreateTimer(A<TimerCallback>._, An<object?>._, TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan)).MustHaveHappened();
    }

    [Fact]
    public async Task fireMultipleDaysOfEvents() {
        OPTIONS.latitude  = 78.92;
        OPTIONS.longitude = 11.93;
        OPTIONS.timeZone  = BERLIN.Id;
        init();

        LocalDate firstDay = new(2024, 9, 9);
        LocalDate lastDay  = new(2024, 9, 11);

        ZonedDateTime now = firstDay.AtStartOfDayInZone(BERLIN);
        A.CallTo(() => clock.GetCurrentInstant()).ReturnsLazily(() => now.ToInstant());

        A.CallTo(() => timeProvider.CreateTimer(A<TimerCallback>._, An<object?>._, A<TimeSpan>._, A<TimeSpan>._))
            .Invokes((TimerCallback _, object? _, TimeSpan dueTime, TimeSpan _) =>
                now = now.Plus(dueTime.ToDuration()))
            .ReturnsLazily<ITimer, TimerCallback, object?, TimeSpan, TimeSpan>((timerCallback, state, _, period) =>
                TimeProvider.System.CreateTimer(timerCallback, state, TimeSpan.FromMilliseconds(1), period));

        solarEventEmitter.solarElevationChanged += (_, change) => {
            if (change.time.Date > lastDay) {
                cts.Cancel();
            }
        };

        using IMonitor<SolarEventEmitterImpl> eventMonitor = solarEventEmitter.Monitor();

        await solarEventEmitter.StartAsync(cts.Token);
        try {
            await solarEventEmitter.ExecuteTask!;
        } catch (OperationCanceledException) { }

        IEventRecording actualEvents = eventMonitor.GetRecordingFor(nameof(SolarEventEmitter.solarElevationChanged));
        IList<SolarElevationChange> expectedEvents = [
            new SolarElevationChange(new LocalDateTime(2024, 9, 9, 5, 7).InZoneStrictly(BERLIN), Sunlight.CIVIL_TWILIGHT, Sunlight.DAYLIGHT, true),
            new SolarElevationChange(new LocalDateTime(2024, 9, 9, 12 + 9, 7).InZoneStrictly(BERLIN), Sunlight.DAYLIGHT, Sunlight.CIVIL_TWILIGHT, false),
            new SolarElevationChange(new LocalDateTime(2024, 9, 10, 0, 29).InZoneStrictly(BERLIN), Sunlight.CIVIL_TWILIGHT, Sunlight.NAUTICAL_TWILIGHT, false),
            new SolarElevationChange(new LocalDateTime(2024, 9, 10, 1, 53).InZoneStrictly(BERLIN), Sunlight.NAUTICAL_TWILIGHT, Sunlight.CIVIL_TWILIGHT, true),
            new SolarElevationChange(new LocalDateTime(2024, 9, 10, 5, 15).InZoneStrictly(BERLIN), Sunlight.CIVIL_TWILIGHT, Sunlight.DAYLIGHT, true),
            new SolarElevationChange(new LocalDateTime(2024, 9, 10, 12 + 8, 58).InZoneStrictly(BERLIN), Sunlight.DAYLIGHT, Sunlight.CIVIL_TWILIGHT, false),
            new SolarElevationChange(new LocalDateTime(2024, 9, 10, 12 + 11, 57).InZoneStrictly(BERLIN), Sunlight.CIVIL_TWILIGHT, Sunlight.NAUTICAL_TWILIGHT, false),
            new SolarElevationChange(new LocalDateTime(2024, 9, 11, 2, 24).InZoneStrictly(BERLIN), Sunlight.NAUTICAL_TWILIGHT, Sunlight.CIVIL_TWILIGHT, true),
            new SolarElevationChange(new LocalDateTime(2024, 9, 11, 5, 24).InZoneStrictly(BERLIN), Sunlight.CIVIL_TWILIGHT, Sunlight.DAYLIGHT, true),
            new SolarElevationChange(new LocalDateTime(2024, 9, 11, 12 + 8, 49).InZoneStrictly(BERLIN), Sunlight.DAYLIGHT, Sunlight.CIVIL_TWILIGHT, false),
            new SolarElevationChange(new LocalDateTime(2024, 9, 11, 12 + 11, 35).InZoneStrictly(BERLIN), Sunlight.CIVIL_TWILIGHT, Sunlight.NAUTICAL_TWILIGHT, false),
            new SolarElevationChange(new LocalDateTime(2024, 9, 12, 2, 46).InZoneStrictly(BERLIN), Sunlight.NAUTICAL_TWILIGHT, Sunlight.CIVIL_TWILIGHT, true),
        ];

        // This is equivalent to .Should().Equal(), but this loop doesn't produce gigantic unreadable outputs when the list has more than a few items in it
        for (int i = 0; i < actualEvents.Count() || i < expectedEvents.Count; i++) {
            actualEvents.Should().HaveCountGreaterOrEqualTo(i + 1, "{0} events should have been fired", expectedEvents.Count);
            expectedEvents.Should().HaveCountGreaterOrEqualTo(i + 1, "{0} events were actually fired", actualEvents.Count());

            OccurredEvent        actual   = actualEvents.ElementAt(i);
            SolarElevationChange expected = expectedEvents[i];

            actual.EventName.Should().Be(nameof(SolarEventEmitter.solarElevationChanged));
            ((SolarElevationChange) actual.Parameters[1]).Should().Be(expected, "index {0}", i);
        }
    }

}