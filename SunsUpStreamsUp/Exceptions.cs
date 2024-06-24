namespace SunsUpStreamsUp;

public abstract class SunsUpStreamsUpException(string? message, Exception? innerException = null): ApplicationException(message, innerException);

public class ObsFailedToConnect(string host, ushort port, bool passwordUsed)
    : SunsUpStreamsUpException($"Failed to connect to OBS at ws://{host}:{port} (a password {(passwordUsed ? "was" : "was not")} used)");