namespace SunsUpStreamsUp;

public readonly struct SolarStreamAction(
    bool           shouldStartStream,
    DateTimeOffset time
) {

    public bool shouldStartStream { get; } = shouldStartStream;
    public DateTimeOffset time { get; } = time;

}