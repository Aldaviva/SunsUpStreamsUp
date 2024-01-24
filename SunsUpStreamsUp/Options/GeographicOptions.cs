namespace SunsUpStreamsUp.Options;

public record GeographicOptions {

    public double latitude { get; set; }
    public double longitude { get; set; }
    public string? timeZone { get; set; }

}