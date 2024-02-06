namespace Tests;

public class ExtensionsTest {

    [Theory]
    [MemberData(nameof(absTimespanData))]
    public void absTimespan(Duration input, Duration expected) {
        input.Abs().Should().Be(expected);
    }

    public static TheoryData<Duration, Duration> absTimespanData => new() {
        { Duration.FromHours(1), Duration.FromHours(1) },
        { Duration.Zero, Duration.Zero },
        { Duration.FromSeconds(-1), Duration.FromSeconds(1) }
    };

}