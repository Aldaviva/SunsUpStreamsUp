using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using SolCalc.Data;
using System.Diagnostics.CodeAnalysis;

namespace SunsUpStreamsUp;

public static class Extensions {

    public static string ToString(this SunlightLevel level, bool humanized = false) => humanized ? level switch {
        SunlightLevel.Night                => "night",
        SunlightLevel.AstronomicalTwilight => "astronomical twilight",
        SunlightLevel.NauticalTwilight     => "nautical twilight",
        SunlightLevel.CivilTwilight        => "civil twilight",
        SunlightLevel.Daylight             => "daylight"
    } : level.ToString();

    public static string ToString(this SolarTimeOfDay timeOfDay, bool humanized = false) => humanized ? timeOfDay switch {
        SolarTimeOfDay.AstronomicalDawn => "astronomical dawn",
        SolarTimeOfDay.NauticalDawn     => "nautical dawn",
        SolarTimeOfDay.CivilDawn        => "civil dawn",
        SolarTimeOfDay.Sunrise          => "sunrise",
        SolarTimeOfDay.Sunset           => "sunset",
        SolarTimeOfDay.CivilDusk        => "civil dusk",
        SolarTimeOfDay.NauticalDusk     => "nautical dusk",
        SolarTimeOfDay.AstronomicalDusk => "astronomical dusk"
    } : timeOfDay.ToString();

    /// <summary>
    /// <para>By default, the .NET host only looks for configuration files in the working directory, not the installation directory, which breaks when you run the program from any other directory.</para>
    /// <para>Fix this by also looking for JSON configuration files in the same directory as this executable.</para>
    /// </summary>
    /// <param name="builder"><see cref="HostApplicationBuilder.Configuration"/></param>
    /// <returns>the same <see cref="IConfigurationBuilder"/> for chaining</returns>
    public static IConfigurationBuilder AlsoSearchForJsonFilesInExecutableDirectory(this IConfigurationBuilder builder) {
        if (Path.GetDirectoryName(Environment.ProcessPath) is { } installationDir) {
            PhysicalFileProvider fileProvider = new(installationDir);

            IEnumerable<(int index, IConfigurationSource source)> sourcesToAdd = builder.Sources.SelectMany<IConfigurationSource, (int, IConfigurationSource)>((src, oldIndex) =>
                src is JsonConfigurationSource { Path: { } path } source
                    ? [(oldIndex, new JsonConfigurationSource { FileProvider = fileProvider, Path = path, Optional = true, ReloadOnChange = source.ReloadOnChange, ReloadDelay = source.ReloadDelay })]
                    : []).ToList();

            int sourcesAdded = 0;
            foreach ((int index, IConfigurationSource? source) in sourcesToAdd) {
                builder.Sources.Insert(index + sourcesAdded++, source);
            }
        }

        return builder;
    }

    /// <summary>
    /// Indicates whether a specified string is <c>null</c>, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="str">The string to test</param>
    /// <returns><c>true</c> if the <paramref name="str"/> parameter is <c>null</c> or <see cref="string.Empty"/>, or if  <paramref name="str"/> consists exclusively of white-space characters.</returns>
    /// <seealso cref="string.IsNullOrWhiteSpace"/>
    public static bool HasText([NotNullWhen(true)] this string? str) => !string.IsNullOrWhiteSpace(str);

}