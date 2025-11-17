using SolCalc.Data;

namespace SunsUpStreamsUp;

public static class Extensions {

    extension(SunlightLevel level) {

        public string ToString(bool humanized = false) => humanized ? level switch {
            SunlightLevel.Night                => "night",
            SunlightLevel.AstronomicalTwilight => "astronomical twilight",
            SunlightLevel.NauticalTwilight     => "nautical twilight",
            SunlightLevel.CivilTwilight        => "civil twilight",
            SunlightLevel.Daylight             => "daylight"
        } : level.ToString();

    }

    extension(SolarTimeOfDay timeOfDay) {

        public string ToString(bool humanized = false) => humanized ? timeOfDay switch {
            SolarTimeOfDay.AstronomicalDawn => "astronomical dawn",
            SolarTimeOfDay.NauticalDawn     => "nautical dawn",
            SolarTimeOfDay.CivilDawn        => "civil dawn",
            SolarTimeOfDay.Sunrise          => "sunrise",
            SolarTimeOfDay.Sunset           => "sunset",
            SolarTimeOfDay.CivilDusk        => "civil dusk",
            SolarTimeOfDay.NauticalDusk     => "nautical dusk",
            SolarTimeOfDay.AstronomicalDusk => "astronomical dusk"
        } : timeOfDay.ToString();

    }

}