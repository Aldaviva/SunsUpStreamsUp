// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using static System.Math;

namespace SunsUpStreamsUp.Math;

/*
 * Source: https://github.com/raminrahimzada/CSharp-Helper-Classes/tree/master/Math/DecimalMath
 */

/// <summary>
/// Analogy of <see cref="System.Math"/> class for decimal types 
/// </summary>
[ExcludeFromCodeCoverage]
public static class DecimalMath {

    /// <summary>
    /// represents PI
    /// </summary>
    public const decimal Pi = 3.14159265358979323846264338327950288419716939937510M;

    /// <summary>
    /// represents PI
    /// </summary>
    public const decimal Epsilon = 0.0000000000000000001M;

    /// <summary>
    /// represents 2*PI
    /// </summary>
    private const decimal PIx2 = 6.28318530717958647692528676655900576839433879875021M;

    /// <summary>
    /// represents E
    /// </summary>
    public const decimal E = 2.7182818284590452353602874713526624977572470936999595749M;

    /// <summary>
    /// represents PI/2
    /// </summary>
    private const decimal PIdiv2 = 1.570796326794896619231321691639751442098584699687552910487M;

    /// <summary>
    /// represents PI/4
    /// </summary>
    private const decimal PIdiv4 = 0.785398163397448309615660845819875721049292349843776455243M;

    /// <summary>
    /// represents 1.0/E
    /// </summary>
    private const decimal Einv = 0.3678794411714423215955237701614608674458111310317678M;

    /// <summary>
    /// log(10,E) factor
    /// </summary>
    private const decimal Log10Inv = 0.434294481903251827651128918916605082294397005803666566114M;

    /// <summary>
    /// Zero
    /// </summary>
    public const decimal Zero = 0.0M;

    /// <summary>
    /// One
    /// </summary>
    public const decimal One = 1.0M;

    /// <summary>
    /// Represents 0.5M
    /// </summary>
    private const decimal Half = 0.5M;

    /// <summary>
    /// Max iterations count in Taylor series
    /// </summary>
    private const int MaxIteration = 100;

    /// <summary>
    /// Analogy of <see cref="Math.Exp"/> method
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Exp(decimal x) {
        int count = 0;

        if (x > One) {
            count =  decimal.ToInt32(decimal.Truncate(x));
            x     -= decimal.Truncate(x);
        }

        if (x < Zero) {
            count = decimal.ToInt32(decimal.Truncate(x) - 1);
            x     = One + (x - decimal.Truncate(x));
        }

        int     iteration = 1;
        decimal result    = One;
        decimal factorial = One;
        decimal cachedResult;
        do {
            cachedResult =  result;
            factorial    *= x / iteration++;
            result       += factorial;
        } while (cachedResult != result);

        if (count == 0) {
            return result;
        }

        return result * PowerN(E, count);
    }

    /// <summary>
    /// Analogy of <see cref="Math.Pow"/> method
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pow"></param>
    /// <returns></returns>
    public static decimal Power(decimal value, decimal pow) {
        if (pow == Zero) return One;
        if (pow == One) return value;
        if (value == One) return One;

        if (value == Zero) {
            if (pow > Zero) {
                return Zero;
            }

            throw new Exception("Invalid Operation: zero base and negative power");
        }

        if (pow == -One) return One / value;

        bool isPowerInteger = IsInteger(pow);
        if (value < Zero && !isPowerInteger) {
            throw new Exception("Invalid Operation: negative base and non-integer power");
        }

        if (isPowerInteger && value > Zero) {
            int powerInt = (int) pow;
            return PowerN(value, powerInt);
        }

        if (isPowerInteger && value < Zero) {
            int powerInt = (int) pow;
            if (powerInt % 2 == 0) {
                return Exp(pow * Log(-value));
            }

            return -Exp(pow * Log(-value));
        }

        return Exp(pow * Log(value));
    }

    private static bool IsInteger(decimal value) {
        long longValue = (long) value;
        return Abs(value - longValue) <= Epsilon;
    }

    /// <summary>
    /// Power to the integer value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="power"></param>
    /// <returns></returns>
    public static decimal PowerN(decimal value, int power) {
        while (true) {
            if (power == Zero) return One;
            if (power < Zero) {
                value = One / value;
                power = -power;
                continue;
            }

            int     q       = power;
            decimal prod    = One;
            decimal current = value;
            while (q > 0) {
                if (q % 2 == 1) {
                    // detects the 1s in the binary expression of power
                    prod = current * prod; // picks up the relevant power
                    q--;
                }

                current *=  current; // value^i -> value^(2*i)
                q       >>= 1;
            }

            return prod;
        }
    }

    /// <summary>
    /// Analogy of <see cref="Math.Log10"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Log10(decimal x) {
        return Log(x) * Log10Inv;
    }

    /// <summary>
    /// Analogy of <see cref="Math.Log(double)"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Log(decimal x) {
        if (x <= Zero) {
            throw new ArgumentException("x must be greater than zero");
        }

        int count = 0;
        while (x >= One) {
            x *= Einv;
            count++;
        }

        while (x <= Einv) {
            x *= E;
            count--;
        }

        x--;
        if (x == Zero) return count;
        decimal result      = Zero;
        int     iteration   = 0;
        decimal y           = One;
        decimal cacheResult = result - One;
        while (cacheResult != result && iteration < MaxIteration) {
            iteration++;
            cacheResult =  result;
            y           *= -x;
            result      += y / iteration;
        }

        return count - result;
    }

    /// <summary>
    /// Analogy of <see cref="Math.Cos"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Cos(decimal x) {
        //truncating to  [-2*PI;2*PI]
        TruncateToPeriodicInterval(ref x);

        // now x in (-2pi,2pi)
        switch (x) {
            case >= Pi and <= PIx2:
                return -Cos(x - Pi);
            case >= -PIx2 and <= -Pi:
                return -Cos(x + Pi);
        }

        x *= x;
        //y=1-x/2!+x^2/4!-x^3/6!...
        decimal xx      = -x * Half;
        decimal y       = One + xx;
        decimal cachedY = y - One; //init cache  with different value
        for (int i = 1; cachedY != y && i < MaxIteration; i++) {
            cachedY = y;
            decimal factor = i * ((i << 1) + 3) + 1; //2i^2+2i+i+1=2i^2+3i+1
            factor =  -Half / factor;
            xx     *= x * factor;
            y      += xx;
        }

        return y;
    }

    /// <summary>
    /// Analogy of <see cref="Math.Tan"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Tan(decimal x) {
        decimal cos = Cos(x);
        if (cos == Zero) throw new ArgumentOutOfRangeException(nameof(x), "Tan(Pi/2) is undefined");
        //calculate sin using cos
        decimal sin = CalculateSinFromCos(x, cos);
        return sin / cos;
    }

    /// <summary>
    /// Helper function for calculating sin(x) from cos(x)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="cos"></param>
    /// <returns></returns>
    private static decimal CalculateSinFromCos(decimal x, decimal cos) {
        decimal moduleOfSin    = Sqrt(One - cos * cos);
        bool    sineIsPositive = IsSignOfSinePositive(x);
        if (sineIsPositive) return moduleOfSin;
        return -moduleOfSin;
    }

    /// <summary>
    /// Analogy of <see cref="Math.Sin"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Sin(decimal x) {
        decimal cos = Cos(x);
        return CalculateSinFromCos(x, cos);
    }

    /// <summary>
    /// Truncates to  [-2*PI;2*PI]
    /// </summary>
    /// <param name="x"></param>
    private static void TruncateToPeriodicInterval(ref decimal x) {
        while (x >= PIx2) {
            int divide = Abs(decimal.ToInt32(x / PIx2));
            x -= divide * PIx2;
        }

        while (x <= -PIx2) {
            int divide = Abs(decimal.ToInt32(x / PIx2));
            x += divide * PIx2;
        }
    }

    private static bool IsSignOfSinePositive(decimal x) {
        //truncating to  [-2*PI;2*PI]
        TruncateToPeriodicInterval(ref x);

        //now x in [-2*PI;2*PI]
        return x switch {
            >= -PIx2 and <= -Pi => true,
            >= -Pi and <= Zero  => false,
            >= Zero and <= Pi   => true,
            >= Pi and <= PIx2   => false,
            //will not be reached
            _ => true
        };
    }

    /// <summary>
    /// Analogy of <see cref="Math.Sqrt"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="epsilon">lasts iteration while error less than this epsilon</param>
    /// <returns></returns>
    public static decimal Sqrt(decimal x, decimal epsilon = Zero) {
        if (x < Zero) throw new OverflowException("Cannot calculate square root from a negative number");
        //initial approximation
        decimal current = (decimal) System.Math.Sqrt((double) x), previous;
        do {
            previous = current;
            if (previous == Zero) return Zero;
            current = (previous + x / previous) * Half;
        } while (Abs(previous - current) > epsilon);

        return current;
    }

    /// <summary>
    /// Analogy of <see cref="Math.Sinh"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Sinh(decimal x) {
        decimal y  = Exp(x);
        decimal yy = One / y;
        return (y - yy) * Half;
    }

    /// <summary>
    /// Analogy of <see cref="Math.Cosh"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Cosh(decimal x) {
        decimal y  = Exp(x);
        decimal yy = One / y;
        return (y + yy) * Half;
    }

    // Provided by System.Math.Sign(decimal)
    /*
    /// <summary>
    /// Analogy of <see cref="Math.Sign"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static int Sign(decimal x) {
        return x < Zero ? -1 : x > Zero ? 1 : 0;
    }
    */

    /// <summary>
    /// Analogy of <see cref="Math.Tanh"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Tanh(decimal x) {
        decimal y  = Exp(x);
        decimal yy = One / y;
        return (y - yy) / (y + yy);
    }

    /*
    /// <summary>
    /// Analogy of <see cref="Math.Abs"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Abs(decimal x) {
        if (x <= Zero) {
            return -x;
        }

        return x;
    }
    */

    /// <summary>
    /// Analogy of <see cref="Math.Asin"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Asin(decimal x) {
        if (x > One || x < -One) {
            throw new ArgumentException("x must be in [-1,1]");
        }

        //known values
        if (x == Zero) return Zero;
        if (x == One) return PIdiv2;
        //asin function is odd function
        if (x < Zero) return -Asin(-x);

        //my optimize trick here

        // used a math formula to speed up :
        // asin(x)=0.5*(pi/2-asin(1-2*x*x)) 
        // if x>=0 is true

        decimal newX = One - 2 * x * x;

        //for calculating new value near to zero than current
        //because we gain more speed with values near to zero
        if (Abs(x) > Abs(newX)) {
            decimal t = Asin(newX);
            return Half * (PIdiv2 - t);
        }

        decimal y      = Zero;
        decimal result = x;
        decimal cachedResult;
        int     i = 1;
        y += result;
        decimal xx = x * x;
        do {
            cachedResult =  result;
            result       *= xx * (One - Half / i);
            y            += result / ((i << 1) + 1);
            i++;
        } while (cachedResult != result);

        return y;
    }

    /// <summary>
    /// Analogy of <see cref="Math.Atan"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal ATan(decimal x) {
        if (x == Zero) return Zero;
        if (x == One) return PIdiv4;
        return Asin(x / Sqrt(One + x * x));
    }

    /// <summary>
    /// Analogy of <see cref="Math.Acos"/>
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Acos(decimal x) {
        if (x == Zero) return PIdiv2;
        if (x == One) return Zero;
        if (x < Zero) return Pi - Acos(-x);
        return PIdiv2 - Asin(x);
    }

    /// <summary>
    /// Analogy of <see cref="Math.Atan2"/>
    /// for more see this
    /// <seealso cref="http://i.imgur.com/TRLjs8R.png"/>
    /// </summary>
    /// <param name="y"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal Atan2(decimal y, decimal x) {
        if (x > Zero) {
            return ATan(y / x);
        }

        if (x < Zero && y >= Zero) {
            return ATan(y / x) + Pi;
        }

        if (x < Zero && y < Zero) {
            return ATan(y / x) - Pi;
        }

        if (x == Zero && y > Zero) {
            return PIdiv2;
        }

        if (x == Zero && y < Zero) {
            return -PIdiv2;
        }

        throw new ArgumentException("invalid atan2 arguments");
    }

}