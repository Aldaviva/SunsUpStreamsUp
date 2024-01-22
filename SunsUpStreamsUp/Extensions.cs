namespace SunsUpStreamsUp;

public static class Extensions {

    public static TimeSpan abs(this TimeSpan input) {
        return input < TimeSpan.Zero ? input.Negate() : input;
    }

}