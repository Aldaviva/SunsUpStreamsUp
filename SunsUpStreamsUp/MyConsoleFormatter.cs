using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using System.Diagnostics.CodeAnalysis;

namespace SunsUpStreamsUp;

[ExcludeFromCodeCoverage]
public class MyConsoleFormatter(IOptions<MyConsoleFormatter.MyConsoleOptions> options): ConsoleFormatter(NAME) {

    public const  string NAME                      = "myConsoleFormatter";
    private const string DEFAULT_TIMESTAMP_PATTERN = "uuuu'-'MM'-'dd HH:mm:ss.fff o<I>";
    private const string PADDING                   = "                                ";
    private const string ANSI_RESET                = "\u001b[0m";

    private static readonly int        MAX_PADDED_CATEGORY_LENGTH = PADDING.Length;
    private static readonly ZonedClock CLOCK                      = SystemClock.Instance.InTzdbSystemDefaultZone();

    private readonly MyConsoleOptions     options          = options.Value;
    private readonly ZonedDateTimePattern timestampPattern = ZonedDateTimePattern.CreateWithCurrentCulture(options.Value.TimestampFormat ?? DEFAULT_TIMESTAMP_PATTERN, DateTimeZoneProviders.Tzdb);

    private int maxCategoryLength;

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter) {
        ZonedDateTime now = CLOCK.GetCurrentZonedDateTime();

        string?    message   = logEntry.State?.ToString();
        Exception? exception = logEntry.Exception;
        if (message is not null || exception is not null) {

            textWriter.Write(formatLevel(logEntry.LogLevel));
            textWriter.Write(options.columnSeparator);
            textWriter.Write(formatTime(now));
            textWriter.Write(options.columnSeparator);
            writeCategory(logEntry, textWriter);
            textWriter.Write(options.columnSeparator);

            if (message is not null) {
                textWriter.Write(message);
            }

            if (message is not null && exception is not null) {
                textWriter.Write("\n   ");
            }

            if (exception is not null) {
                textWriter.Write(exception.ToString().Replace("\n", "\n   "));
            }

            textWriter.WriteLine(ANSI_RESET);
        }
    }

    private void writeCategory<TState>(LogEntry<TState> logEntry, TextWriter textWriter) {
        int lastSeparatorPosition = options.includeNamespaces ? -1 : logEntry.Category.LastIndexOf('.', logEntry.Category.Length - 2);

        ReadOnlySpan<char> category = lastSeparatorPosition != -1 ? logEntry.Category.AsSpan(lastSeparatorPosition + 1) : logEntry.Category.AsSpan();

        int categoryLength = category.Length;
        maxCategoryLength = System.Math.Max(maxCategoryLength, categoryLength);
        textWriter.Write(category);

        if (categoryLength >= maxCategoryLength) {
            maxCategoryLength = categoryLength;
        } else {
            textWriter.Write(PADDING.AsSpan(0, System.Math.Max(0, System.Math.Min(maxCategoryLength, MAX_PADDED_CATEGORY_LENGTH) - categoryLength)));
        }
    }

    private string formatTime(ZonedDateTime time) => timestampPattern.Format(time);

    private static string formatLevel(LogLevel level) => level switch {
        LogLevel.Trace       => "\u001b[0;90m t",
        LogLevel.Debug       => " d",
        LogLevel.Information => "\u001b[0;36m i",
        LogLevel.Warning     => "\u001b[30;43m W",
        LogLevel.Error       => "\u001b[97;41m E",
        LogLevel.Critical    => "\u001b[97;41m C",
        _                    => "  "
    };

    public class MyConsoleOptions: ConsoleFormatterOptions {

        public bool includeNamespaces { get; set; }
        public string columnSeparator { get; set; } = " | ";

    }

}