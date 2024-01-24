namespace Tests;

public class EnumTests {

    private static readonly DateTimeZone BERLIN = DateTimeZoneProviders.Tzdb["Europe/Berlin"];

    [Fact]
    public void sunlightChangedInitializer() {
        new SunlightChanged { startTime = default, sunlightLevel = Sunlight.DAYLIGHT, isSunRising = true, solarElevation = 90 }.sunlightLevel.Should().Be(Sunlight.DAYLIGHT);
    }

    [Fact]
    public void solarElevationChangeInitializer() {
        new SolarElevationChange { isSunRising = false, newSunlight = Sunlight.CIVIL_TWILIGHT, oldSunlight = Sunlight.DAYLIGHT, time = default }.newSunlight.Should().Be(Sunlight.CIVIL_TWILIGHT);
    }

    [Fact]
    public void solarElevationChangeEquality() {
        SolarElevationChange a = new(new LocalDateTime(2024, 9, 9, 12 + 9, 13).InZoneStrictly(BERLIN), Sunlight.DAYLIGHT, Sunlight.CIVIL_TWILIGHT, false);
        SolarElevationChange b = new(new LocalDateTime(2024, 9, 9, 12 + 9, 13).InZoneStrictly(BERLIN), Sunlight.DAYLIGHT, Sunlight.CIVIL_TWILIGHT, false);

        (a == b).Should().BeTrue();
        a.Should().Be(b);
    }

}