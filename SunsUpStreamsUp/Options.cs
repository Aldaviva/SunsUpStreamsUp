namespace SunsUpStreamsUp;

public class Options {

    public double latitude { get; set; }
    public double longitude { get; set; }
    public string obsHostname { get; set; } = "localhost";
    public ushort obsPort { get; set; } = 4455;
    public string obsPassword { get; set; } = string.Empty;

}