using SolCalc.Data;

namespace Tests;

public class ExtensionsTest {

    [Theory]
    [InlineData(SolarTimeOfDay.AstronomicalDawn, "astronomical dawn")]
    [InlineData(SolarTimeOfDay.NauticalDawn, "nautical dawn")]
    [InlineData(SolarTimeOfDay.CivilDawn, "civil dawn")]
    [InlineData(SolarTimeOfDay.Sunrise, "sunrise")]
    [InlineData(SolarTimeOfDay.Sunset, "sunset")]
    [InlineData(SolarTimeOfDay.CivilDusk, "civil dusk")]
    [InlineData(SolarTimeOfDay.NauticalDusk, "nautical dusk")]
    [InlineData(SolarTimeOfDay.AstronomicalDusk, "astronomical dusk")]
    public void solarTimeOfDayToString(SolarTimeOfDay solarTimeOfDay, string expected) {
        solarTimeOfDay.ToString(true).Should().Be(expected);
    }

}