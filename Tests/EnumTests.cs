using SolCalc.Data;

namespace Tests;

public class EnumTests {

    private static readonly DateTimeZone BERLIN = DateTimeZoneProviders.Tzdb["Europe/Berlin"];

    [Fact]
    public void solarElevationChangeEquality() {
        SolarElevationChange a = new(new LocalDateTime(2024, 9, 9, 12 + 9, 13).InZoneStrictly(BERLIN), SunlightLevel.Daylight, SunlightLevel.CivilTwilight, false);
        SolarElevationChange b = new(new LocalDateTime(2024, 9, 9, 12 + 9, 13).InZoneStrictly(BERLIN), SunlightLevel.Daylight, SunlightLevel.CivilTwilight, false);

        (a == b).Should().BeTrue();
        a.Should().Be(b);
    }

}