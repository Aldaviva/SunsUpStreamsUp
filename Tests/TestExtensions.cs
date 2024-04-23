namespace Tests;

public static class TestExtensions {

    public static TimeSpan abs(this TimeSpan input) {
        return input < TimeSpan.Zero ? input.Negate() : input;
    }

}