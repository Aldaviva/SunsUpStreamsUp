namespace SunsUpStreamsUp;

public readonly struct SolarStreamAction(
    bool     shouldStartStream,
    DateTime time
) {

    public bool shouldStartStream { get; } = shouldStartStream;
    public DateTime time { get; } = time;

}