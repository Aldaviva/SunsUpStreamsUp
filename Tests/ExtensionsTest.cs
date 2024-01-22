using FluentAssertions;
using SunsUpStreamsUp;

namespace Tests;

public class ExtensionsTest {

    private static readonly TimeZoneInfo PACIFIC_TIME = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");

    [Theory]
    [MemberData(nameof(absTimespanData))]
    public void absTimespan(TimeSpan input, TimeSpan expected) {
        input.abs().Should().Be(expected);
    }

    public static TheoryData<TimeSpan, TimeSpan> absTimespanData => new() {
        { TimeSpan.FromHours(1), TimeSpan.FromHours(1) },
        { TimeSpan.Zero, TimeSpan.Zero },
        { TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(1) }
    };

    [Theory]
    [MemberData(nameof(toStartOfDayData))]
    public void toStartOfDay(DateTimeOffset input, TimeZoneInfo zone, DateTimeOffset expected) {
        input.toStartOfDay(zone).Should().Be(expected);
    }

    public static TheoryData<DateTimeOffset, TimeZoneInfo, DateTimeOffset> toStartOfDayData => new() {
        { new DateTimeOffset(2024, 1, 22, 2, 15, 17, TimeSpan.FromHours(-8)), PACIFIC_TIME, new DateTimeOffset(2024, 1, 22, 0, 0, 0, TimeSpan.FromHours(-8)) },      // pst → pst
        { new DateTimeOffset(2024, 8, 17, 12 + 4, 30, 17, TimeSpan.FromHours(-7)), PACIFIC_TIME, new DateTimeOffset(2024, 8, 17, 0, 0, 0, TimeSpan.FromHours(-7)) }, // pdt → pdt
        { new DateTimeOffset(2024, 3, 10, 3, 0, 0, TimeSpan.FromHours(-7)), PACIFIC_TIME, new DateTimeOffset(2024, 3, 10, 0, 0, 0, TimeSpan.FromHours(-8)) },        // pdt → pst
        { new DateTimeOffset(2024, 11, 3, 3, 0, 0, TimeSpan.FromHours(-8)), PACIFIC_TIME, new DateTimeOffset(2024, 11, 3, 0, 0, 0, TimeSpan.FromHours(-7)) },        // pst → pdt
    };

    [Theory]
    [MemberData(nameof(withZoneData))]
    public void withZone(DateTime dateTime, TimeZoneInfo zone, DateTimeOffset expected) {
        dateTime.withZone(zone).Should().Be(expected);
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTimeOffset> withZoneData => new() {
        { new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local), PACIFIC_TIME, new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.FromHours(-8)) },
        { new DateTime(2024, 1, 22, 2, 34, 0, DateTimeKind.Local), PACIFIC_TIME, new DateTimeOffset(2024, 1, 22, 2, 34, 0, TimeSpan.FromHours(-8)) },
        { new DateTime(1988, 8, 17, 12 + 4, 30, 0, DateTimeKind.Local), PACIFIC_TIME, new DateTimeOffset(1988, 8, 17, 12 + 4, 30, 0, TimeSpan.FromHours(-7)) },
    };

}