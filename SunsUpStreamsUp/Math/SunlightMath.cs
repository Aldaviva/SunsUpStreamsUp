using NodaTime;

namespace SunsUpStreamsUp.Math;

public readonly record struct SunlightChanged(
    ZonedDateTime startTime,
    Sunlight      sunlightLevel,
    bool          isSunRising,
    decimal       solarElevation
);

public enum Sunlight {

    DAYLIGHT,
    CIVIL_TWILIGHT,
    NAUTICAL_TWILIGHT,
    ASTRONOMICAL_TWILIGHT,
    NIGHT

}

public static class SunlightExtensions {

    public static Sunlight forSolarElevation(decimal solarElevationDegrees) => solarElevationDegrees switch {
        >= 0   => Sunlight.DAYLIGHT,
        >= -6  => Sunlight.CIVIL_TWILIGHT,
        >= -12 => Sunlight.NAUTICAL_TWILIGHT,
        >= -18 => Sunlight.ASTRONOMICAL_TWILIGHT,
        _      => Sunlight.NIGHT
    };

    public static string ToString(this Sunlight sunlight, bool humanized = true) => humanized ? sunlight switch {
        Sunlight.DAYLIGHT              => "daylight",
        Sunlight.CIVIL_TWILIGHT        => "civil twilight",
        Sunlight.NAUTICAL_TWILIGHT     => "nautical twilight",
        Sunlight.ASTRONOMICAL_TWILIGHT => "astronomical twilight",
        Sunlight.NIGHT                 => "night"
    } : sunlight.ToString();

}

public static class SunlightMath {

    private static readonly Duration INCREMENT = Duration.FromMinutes(1);

    public static Sunlight getSunlightForTime(ZonedDateTime time, double latitude, double longitude) => SunlightExtensions.forSolarElevation(SolarMath.getSolarElevation(time, latitude, longitude));

    public static IEnumerable<SunlightChanged> getDailySunlightChanges(LocalDate date, DateTimeZone zone, double latitude, double longitude, decimal? previousSolarElevation) {
        ZonedDateTime time = date.AtStartOfDayInZone(zone);
        if (previousSolarElevation == null) {
            previousSolarElevation = SolarMath.getSolarElevation(time, latitude, longitude);
            time                   = time.Plus(INCREMENT);
        }

        Sunlight previousSunlight = SunlightExtensions.forSolarElevation(previousSolarElevation.Value);

        while (time.Day == date.Day) {
            decimal  currentSolarElevation = SolarMath.getSolarElevation(time, latitude, longitude);
            Sunlight currentSunlight       = SunlightExtensions.forSolarElevation(currentSolarElevation);

            if (previousSunlight != currentSunlight) {
                yield return new SunlightChanged(time, currentSunlight, previousSolarElevation < currentSolarElevation, currentSolarElevation);
                previousSunlight = currentSunlight;
            }

            previousSolarElevation = currentSolarElevation;
            time                   = time.Plus(INCREMENT);
        }
    }

}