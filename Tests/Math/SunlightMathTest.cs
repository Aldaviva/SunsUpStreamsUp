namespace Tests.Math;

public class SunlightMathTest {

    private static readonly DateTimeZone LOS_ANGELES = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
    private static readonly DateTimeZone BERLIN      = DateTimeZoneProviders.Tzdb["Europe/Berlin"];

    [Theory]
    [MemberData(nameof(elevationData))]
    public void elevation(ZonedDateTime time, double latitude, double longitude, decimal expectedAzimuthDegrees, decimal expectedElevationDegrees) {
        const decimal TOLERANCE = 0.01m;
        (decimal azimuth, decimal elevation) = SolarMath.getSolarAzimuthAndElevation(time, latitude, longitude);
        azimuth.Should().BeApproximately(expectedAzimuthDegrees, TOLERANCE);
        elevation.Should().BeApproximately(expectedElevationDegrees, TOLERANCE);

        SolarMath.getSolarAzimuth(time, latitude, longitude).Should().BeApproximately(expectedAzimuthDegrees, TOLERANCE);
        SolarMath.getSolarElevation(time, latitude, longitude).Should().BeApproximately(expectedElevationDegrees, TOLERANCE);
    }

    public static TheoryData<ZonedDateTime, double, double, decimal, decimal> elevationData => new() {
        { new LocalDateTime(2024, 1, 23, 6, 0).InZoneStrictly(LOS_ANGELES), 37.77, -122.42, 102.59m, -15.87m }
    };

    [Theory]
    [MemberData(nameof(getSolarEventsForDateData))]
    public void getSolarEventsForDate(LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<SunlightChanged> expecteds) {
        IList<SunlightChanged> actuals = SunlightMath.getDailySunlightChanges(date, zone, latitude, longitude, null).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            SunlightChanged actual   = actuals[i];
            SunlightChanged expected = expecteds[i];

            // precision is worse at latitudes farther from the equator because the reference values from timeanddate.com don't seem to take atmospheric refraction of sunlight into account, but the NOAA code under test does
            actual.startTime.Should().BeCloseTo(expected.startTime, Duration.FromMinutes(7), "event {0} time", i);
            actual.isSunRising.Should().Be(expected.isSunRising, "event {0} isDawn", i);
            actual.sunlightLevel.Should().Be(expected.sunlightLevel, "event {0} lightPeriod", i);
            actual.solarElevation.Should().BeApproximately(expected.solarElevation, 0.2m);
        }
    }

    public static TheoryData<LocalDate, DateTimeZone, double, double, IList<SunlightChanged>> getSolarEventsForDateData => new() {
        {
            new LocalDate(2024, 1, 23), LOS_ANGELES, 37.35, -121.95, [
                new SunlightChanged(new LocalDateTime(2024, 1, 23, 5, 46).InZoneStrictly(LOS_ANGELES), Sunlight.ASTRONOMICAL_TWILIGHT, true, -18),
                new SunlightChanged(new LocalDateTime(2024, 1, 23, 6, 17).InZoneStrictly(LOS_ANGELES), Sunlight.NAUTICAL_TWILIGHT, true, -12),
                new SunlightChanged(new LocalDateTime(2024, 1, 23, 6, 49).InZoneStrictly(LOS_ANGELES), Sunlight.CIVIL_TWILIGHT, true, -6),
                new SunlightChanged(new LocalDateTime(2024, 1, 23, 7, 17).InZoneStrictly(LOS_ANGELES), Sunlight.DAYLIGHT, true, 0),
                new SunlightChanged(new LocalDateTime(2024, 1, 23, 12 + 5, 22).InZoneStrictly(LOS_ANGELES), Sunlight.CIVIL_TWILIGHT, false, 0),
                new SunlightChanged(new LocalDateTime(2024, 1, 23, 12 + 5, 50).InZoneStrictly(LOS_ANGELES), Sunlight.NAUTICAL_TWILIGHT, false, -6),
                new SunlightChanged(new LocalDateTime(2024, 1, 23, 12 + 6, 21).InZoneStrictly(LOS_ANGELES), Sunlight.ASTRONOMICAL_TWILIGHT, false, -12),
                new SunlightChanged(new LocalDateTime(2024, 1, 23, 12 + 6, 52).InZoneStrictly(LOS_ANGELES), Sunlight.NIGHT, false, -18)
            ]
        }, {
            new LocalDate(2024, 9, 10), BERLIN, 78.92, 11.93, [
                new SunlightChanged(new LocalDateTime(2024, 9, 10, 0, 22).InZoneStrictly(BERLIN), Sunlight.NAUTICAL_TWILIGHT, false, -6),
                new SunlightChanged(new LocalDateTime(2024, 9, 10, 1, 58).InZoneStrictly(BERLIN), Sunlight.CIVIL_TWILIGHT, true, -6),
                new SunlightChanged(new LocalDateTime(2024, 9, 10, 5, 8).InZoneStrictly(BERLIN), Sunlight.DAYLIGHT, true, 0),
                new SunlightChanged(new LocalDateTime(2024, 9, 10, 12 + 9, 3).InZoneStrictly(BERLIN), Sunlight.CIVIL_TWILIGHT, false, 0),
                new SunlightChanged(new LocalDateTime(2024, 9, 10, 12 + 11, 53).InZoneStrictly(BERLIN), Sunlight.NAUTICAL_TWILIGHT, false, -6)
            ]
        }, {
            new LocalDate(2024, 4, 17), BERLIN, 78.92, 11.93, []
        }
    };

}