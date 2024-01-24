using NodaTime;

namespace SunsUpStreamsUp;

public static class Extensions {

    public static Duration abs(this Duration input) {
        return input < Duration.Zero ? Duration.Negate(input) : input;
    }

    public static Duration? abs(this Duration? input) {
        return input != null && input < Duration.Zero ? Duration.Negate(input.Value) : input;
    }

}