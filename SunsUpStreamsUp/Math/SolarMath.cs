using NodaTime;
using static System.Math;
using static SunsUpStreamsUp.Math.DecimalMath;

namespace SunsUpStreamsUp.Math;

/// <summary>
/// <para>Ported from Solar Calculator by the NOAA Earth System Research Laboratories' Global Monitoring Laboratory</para>
/// <para>Web application: <see href="https://gml.noaa.gov/grad/solcalc/"/></para>
/// <para>JavaScript source: <see href="https://gml.noaa.gov/grad/solcalc/main.js"/></para>
/// <para>This algorithm used by the webapp is supposed to be more accurate than the spreadsheet versions.</para>
/// </summary>
public static class SolarMath {

    private readonly record struct TimeAndPlace {

        public TimeAndPlace(ZonedDateTime dateTime, double latitude, double longitude) {
            localTime      = (decimal) dateTime.TimeOfDay.Minus(LocalTime.Midnight).ToDuration().TotalMinutes;
            timeZoneOffset = (decimal) dateTime.Offset.ToTimeSpan().TotalHours;
            julianDateTime = calcTimeJulianCent(getJd(dateTime.Date) + localTime / 1440.0m - timeZoneOffset / 24.0m);
            this.latitude  = (decimal) latitude;
            this.longitude = (decimal) longitude;
        }

        public decimal julianDateTime { get; }
        public decimal localTime { get; }
        public decimal latitude { get; }
        public decimal longitude { get; }
        public decimal timeZoneOffset { get; }

    }

    public static decimal getSolarElevation(ZonedDateTime dateTime, double latitude, double longitude) {
        TimeAndPlace t = new(dateTime, latitude, longitude);
        return calcEl(t.julianDateTime, t.localTime, t.latitude, t.longitude, t.timeZoneOffset);
    }

    public static decimal getSolarAzimuth(ZonedDateTime dateTime, double latitude, double longitude) {
        TimeAndPlace t = new(dateTime, latitude, longitude);
        return calcAz(t.julianDateTime, t.localTime, t.latitude, t.longitude, t.timeZoneOffset);
    }

    public static (decimal azimuth, decimal elevation) getSolarAzimuthAndElevation(ZonedDateTime dateTime, double latitude, double longitude) {
        TimeAndPlace t = new(dateTime, latitude, longitude);
        return calcAzEl(t.julianDateTime, t.localTime, t.latitude, t.longitude, t.timeZoneOffset);
    }

    private static decimal calcTimeJulianCent(decimal julianDate) => (julianDate - 2451545.0m) / 36525.0m;

    private static decimal getJd(LocalDate date) {
        (int year, int month, int day) = date;

        if (month <= 2) {
            year  -= 1;
            month += 12;
        }

        decimal century = Floor(year / 100m);
        return Floor(365.25m * (year + 4716m)) + Floor(30.6001m * (month + 1)) + day + (2 - century) + Floor(century / 4) - 1524.5m;
    }

    private static decimal calcEl(decimal t, decimal localtime, decimal latitude, decimal longitude, decimal zone) {
        return calcEl(calcAzElCommon(t, localtime, latitude, longitude, zone).zenith);
    }

    private static decimal calcEl(decimal zenith) {
        // Atmospheric Refraction correction
        return 90.0m - (zenith - calcRefraction(90.0m - zenith));
    }

    private static decimal calcAz(decimal t, decimal localtime, decimal latitude, decimal longitude, decimal zone) {
        (decimal zenith, decimal latitudeRad, decimal thetaRad, decimal hourAngle) = calcAzElCommon(t, localtime, latitude, longitude, zone);
        return calcAz(zenith, latitudeRad, thetaRad, hourAngle);
    }

    private static decimal calcAz(decimal zenith, decimal latitudeRad, decimal thetaRad, decimal hourAngle) {
        decimal zenithRad = degToRad(zenith);
        decimal azDenom   = Cos(latitudeRad) * Sin(zenithRad);
        decimal azimuth;
        if (Abs(azDenom) > 0.001m) {
            decimal azRad = (Sin(latitudeRad) * Cos(zenithRad) - Sin(thetaRad)) / azDenom;
            if (Abs(azRad) > 1.0m) {
                azRad = azRad < 0 ? -1.0m : 1.0m;
            }

            azimuth = 180.0m - radToDeg(Acos(azRad));
            if (hourAngle > 0.0m) {
                azimuth = -azimuth;
            }
        } else {
            azimuth = radToDeg(latitudeRad) > 0.0m ? 180.0m : 0.0m;
        }

        if (azimuth < 0.0m) {
            azimuth += 360.0m;
        }

        return azimuth;
    }

    private static (decimal azimuth, decimal elevation) calcAzEl(decimal t, decimal localtime, decimal latitude, decimal longitude, decimal zone) {
        (decimal zenith, decimal latitudeRad, decimal thetaRad, decimal hourAngle) = calcAzElCommon(t, localtime, latitude, longitude, zone);
        return (azimuth: calcAz(zenith, latitudeRad, thetaRad, hourAngle), elevation: calcEl(zenith));
    }

    private static (decimal zenith, decimal latitudeRad, decimal thetaRad, decimal hourAngle) calcAzElCommon(decimal t, decimal localtime, decimal latitude, decimal longitude, decimal zone) {
        decimal latitudeRad   = degToRad(latitude);
        decimal thetaRad      = degToRad(calcSunDeclination(t));
        decimal trueSolarTime = localtime + (calcEquationOfTime(t) + 4.0m * longitude - 60.0m * zone);
        while (trueSolarTime > 1440) {
            trueSolarTime -= 1440;
        }

        decimal hourAngle = trueSolarTime / 4.0m - 180.0m;

        if (hourAngle < -180) {
            hourAngle += 360.0m;
        }

        decimal csz = Sin(latitudeRad) * Sin(thetaRad) + Cos(latitudeRad) * Cos(thetaRad) * Cos(degToRad(hourAngle));
        switch (csz) {
            case > 1.0m:
                csz = 1.0m;
                break;
            case < -1.0m:
                csz = -1.0m;
                break;
        }

        decimal zenith = radToDeg(Acos(csz));

        return (zenith, latitudeRad, thetaRad, hourAngle);
    }

    private static decimal calcRefraction(decimal elev) {
        decimal correction;
        if (elev > 85.0m) {
            correction = 0.0m;
        } else {
            decimal te = Tan(degToRad(elev));
            correction = elev switch {
                > 5.0m    => 58.1m / te - 0.07m / (te * te * te) + 0.000086m / (te * te * te * te * te),
                > -0.575m => 1735.0m + elev * (-518.2m + elev * (103.4m + elev * (-12.79m + elev * 0.711m))),
                _         => -20.774m / te
            } / 3600.0m;
        }

        return correction;
    }

    private static decimal calcEccentricityEarthOrbit(decimal t) => 0.016708634m - t * (0.000042037m + 0.0000001267m * t); // unitless

    private static decimal calcSunEqOfCenter(decimal t) {
        decimal mrad = degToRad(calcGeomMeanAnomalySun(t));
        return Sin(mrad) * (1.914602m - t * (0.004817m + 0.000014m * t)) + Sin(mrad + mrad) * (0.019993m - 0.000101m * t) + Sin(mrad + mrad + mrad) * 0.000289m; // in degrees
    }

    private static decimal calcGeomMeanAnomalySun(decimal t) => 357.52911m + t * (35999.05029m - 0.0001537m * t); // in degrees

    private static decimal calcSunDeclination(decimal t) {
        decimal x = degToRad(125.04m - 1934.136m * t);
        return radToDeg(Asin(Sin(degToRad(calcObliquityCorrection(t, x))) * Sin(degToRad(calcSunApparentLong(t, x)))));
        // in degrees
    }

    private static decimal calcSunApparentLong(decimal t, decimal x) => calcSunTrueLong(t) - 0.00569m - 0.00478m * Sin(x); // in degrees

    private static decimal calcSunTrueLong(decimal t) => calcGeomMeanLongSun(t) + calcSunEqOfCenter(t); // in degrees

    private static decimal calcGeomMeanLongSun(decimal t) {
        decimal l0 = 280.46646m + t * (36000.76983m + t * 0.0003032m);
        while (l0 > 360.0m) {
            l0 -= 360.0m;
        }

        while (l0 < 0.0m) {
            l0 += 360.0m;
        }

        return l0; // in degrees
    }

    private static decimal calcObliquityCorrection(decimal t, decimal x) => calcMeanObliquityOfEcliptic(t) + 0.00256m * Cos(x); // in degrees

    private static decimal calcMeanObliquityOfEcliptic(decimal t) => 23.0m + (26.0m + (21.448m - t * (46.8150m + t * (0.00059m - t * 0.001813m))) / 60.0m) / 60.0m; // in degrees

    private static decimal radToDeg(decimal angleRad) => 180.0m * angleRad / Pi;

    private static decimal degToRad(decimal angleDeg) => Pi * angleDeg / 180.0m;

    private static decimal calcEquationOfTime(decimal t) {
        decimal l0Rad = degToRad(calcGeomMeanLongSun(t));
        decimal e     = calcEccentricityEarthOrbit(t);
        decimal mRad  = degToRad(calcGeomMeanAnomalySun(t));
        decimal sinm  = Sin(mRad);
        decimal y     = Tan(degToRad(calcObliquityCorrection(t, degToRad(125.04m - 1934.136m * t))) / 2.0m);
        y *= y;

        return radToDeg(y * Sin(2.0m * l0Rad) - 2.0m * e * sinm + 4.0m * e * y * sinm * Cos(2.0m * l0Rad) - 0.5m * y * y * Sin(4.0m * l0Rad)
            - 1.25m * e * e * Sin(2.0m * mRad)) * 4.0m; // in minutes of time
    }

}