namespace SunsUpStreamsUp;

public static class Extensions {

    public static TimeSpan abs(this TimeSpan input) {
        return input < TimeSpan.Zero ? input.Negate() : input;
    }

    public static DateTimeOffset toStartOfDay(this DateTimeOffset input, TimeZoneInfo zone) {
        DateTimeOffset newTime = input.Subtract(input.TimeOfDay);
        return new DateTimeOffset(newTime.DateTime, zone.GetUtcOffset(newTime));
    }

    /// <summary>
    /// <para>Assigns a time zone to a local, zoneless <see cref="DateTime"/> without changing any of the other fields. Useful if you happen to know the zone that a local DateTime is actually in.</para>
    /// <para>Example: 1970-01-01 00:00 → 1970-01-01 00:00 -0800</para>
    /// <para>If you instead want to convert between two zones while modifying fields to represent the same instant, use <see cref="System.TimeZoneInfo.ConvertTime(DateTimeOffset, TimeZoneInfo)"/>.</para>
    /// </summary>
    public static DateTimeOffset withZone(this DateTime dateTime, TimeZoneInfo zone) {
        return new DateTimeOffset(new DateTime(dateTime.Ticks, DateTimeKind.Unspecified), zone.GetUtcOffset(dateTime));
    }

    public static DateOnly dateOnly(this DateTimeOffset dateTime) {
        return DateOnly.FromDateTime(dateTime.DateTime);
    }

    public static TimeOnly timeOnly(this DateTimeOffset dateTime) {
        return TimeOnly.FromDateTime(dateTime.DateTime);
    }

}