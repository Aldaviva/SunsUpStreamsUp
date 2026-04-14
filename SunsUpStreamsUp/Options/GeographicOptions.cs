using SolCalc.Data;

namespace SunsUpStreamsUp.Options;

public sealed record GeographicOptions {

    public double latitude { get; init; }
    public double longitude { get; init; }
    public string? timeZone { get; init; }
    public SunlightLevel? minimumSunlightLevel { get; init; }

}