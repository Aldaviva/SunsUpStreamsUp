using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using SolCalc.Data;

namespace Tests;

public class ExtensionsTest {

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("a", true)]
    [InlineData("a b", true)]
    public void stringHasText(string? str, bool expected) {
        str.HasText().Should().Be(expected);
    }

    [Fact]
    public void AlsoSearchForJsonFilesInExecutableDirectory() {
        IConfigurationBuilder configBuilder   = A.Fake<IConfigurationBuilder>();
        string                installationDir = Path.GetDirectoryName(Environment.ProcessPath)!;
        IList<IConfigurationSource> sources = [
            new JsonConfigurationSource { Path = "abc", ReloadOnChange = true, ReloadDelay = 123 }
        ];

        A.CallTo(() => configBuilder.Sources).Returns(sources);

        configBuilder.AlsoSearchForJsonFilesInExecutableDirectory();

        JsonConfigurationSource actual = (JsonConfigurationSource) sources[0];
        actual.Path.Should().Be("abc");
        actual.ReloadOnChange.Should().BeTrue();
        actual.ReloadDelay.Should().Be(123);
        actual.Optional.Should().BeTrue();
        ((PhysicalFileProvider) actual.FileProvider!).Root.Should().Be(installationDir + Path.DirectorySeparatorChar);
    }

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