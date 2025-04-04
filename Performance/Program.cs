using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNetVisualizer;
using NodaTime;
using SunsUpStreamsUp.Math;
using System.Diagnostics;

namespace Performance;

internal static class Program {

    public static void Main(string[] args) {
        Summary summary = BenchmarkRunner.Run<SolCalcBenchmarks>(args: args);

        string reportPath = Path.Combine(summary.ResultsDirectoryPath, summary.BenchmarksCases.First().Descriptor.Type.FullName + "-report-rich.html");
        Console.WriteLine($"\nHTML report: {reportPath}");
        if (File.Exists(reportPath)) {
            Process.Start(new ProcessStartInfo(reportPath) { UseShellExecute = true })?.Dispose();
        }
    }

}

[RichHtmlExporter(
    title: nameof(SolarMath),
    spectrumColumns: ["Mean"],
    groupByColumns: ["Categories"],
    highlightGroups: false)]
[CategoriesColumn]
[ShortRunJob]
public class SolCalcBenchmarks {

    private const double LATITUDE  = 37.77;
    private const double LONGITUDE = -122.42;

    private DateTimeZone  americaLosAngeles = null!;
    private ZonedDateTime dateTime;
    private LocalDate     date;

    [GlobalSetup]
    public void setup() {
        americaLosAngeles = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        dateTime          = new LocalDateTime(2024, 1, 23, 6, 0).InZoneStrictly(americaLosAngeles);
        date              = new LocalDate(2024, 1, 23);
    }

    [Benchmark]
    [BenchmarkCategory(nameof(SolarMath.getSolarElevation))]
    public decimal getElevation() {
        return SolarMath.getSolarElevation(dateTime, LATITUDE, LONGITUDE);
    }

    [Benchmark]
    [BenchmarkCategory(nameof(SunlightMath.getDailySunlightChanges))]
    public IList<SunlightChanged> getSolarEvents() {
        return SunlightMath.getDailySunlightChanges(date, americaLosAngeles, LATITUDE, LONGITUDE).ToList();
    }

    /*[Benchmark]
    public decimal getElevationInlined() {
        return SolCalcInlining.calculate(DATE_TIME, LATITUDE, LONGITUDE).elevation;
    }*/

    /*[Benchmark]
    public decimal getElevationOptimized() {
        return SolCalcOptimized.calculate(DATE_TIME, LATITUDE, LONGITUDE).elevation;
    }*/

}