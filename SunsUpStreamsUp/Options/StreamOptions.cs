namespace SunsUpStreamsUp.Options;

public record StreamOptions {

    public string obsHostname { get; init; } = "localhost";
    public ushort obsPort { get; init; } = 4455;
    public string obsPassword { get; init; } = string.Empty;

}