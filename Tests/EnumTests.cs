using SolCalc.Data;

namespace Tests;

public class EnumTests {

    private static readonly DateTimeZone BERLIN = DateTimeZoneProviders.Tzdb["Europe/Berlin"];

    [Fact]
    public void solarElevationChangeEquality() {
        SunlightChange a = new(new LocalDateTime(2024, 9, 9, 12 + 9, 13).InZoneStrictly(BERLIN), SolarTimeOfDay.Sunset);
        SunlightChange b = new(new LocalDateTime(2024, 9, 9, 12 + 9, 13).InZoneStrictly(BERLIN), SolarTimeOfDay.Sunset);

        (a == b).Should().BeTrue();
        a.Should().Be(b);
    }

}