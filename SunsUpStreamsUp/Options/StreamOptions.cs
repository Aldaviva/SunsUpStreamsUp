namespace SunsUpStreamsUp.Options;

public record StreamOptions {

    public string obsHostname { get; set; } = "localhost";
    public ushort obsPort { get; set; } = 4455;
    public string obsPassword { get; set; } = string.Empty;

}