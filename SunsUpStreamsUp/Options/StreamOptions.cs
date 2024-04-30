namespace SunsUpStreamsUp.Options;

public record StreamOptions {

    public string obsHostname { get; set; } = "localhost";
    public ushort obsPort { get; set; } = 4455;
    public string obsPassword { get; set; } = string.Empty;
    public bool replaceExistingStream { get; set; }

    /// <summary>
    /// To provision an OAuth client app for Twitch:
    /// <list type="number">
    /// <item><description>Go to <see href="https://dev.twitch.tv/console/apps/create" /></description></item>
    /// <item><description>Choose any name</description></item>
    /// <item><description>Set the redirection URL to <c>http://localhost</c></description></item>
    /// <item><description>Choose any Category, such as Application Integration</description></item>
    /// <item><description>Set Client Type to Confidential</description></item>
    /// <item><description>Click Create</description></item>
    /// <item><description>Click Manage on the new Application</description></item>
    /// <item><description>Copy the Client ID and paste it into the <c>stream.twitchClientId</c> property in <c>appsettings.json</c></description></item>
    /// <item><description>Click New Secret</description></item>
    /// <item><description>Copy the Client Secret and paste it into the <c>stream.twitchClientSecret</c> in <c>appsettings.json</c></description></item>
    /// <item><description>Set the <c>stream.twitchUsername</c> to your Twitch username in <c>appsettings.json</c></description></item>
    /// </list>
    /// </summary>
    public string? twitchClientId { get; set; }

    /// <inheritdoc cref="twitchClientId"/>
    public string? twitchClientSecret { get; set; }

    /// <inheritdoc cref="twitchClientId"/>
    public string? twitchUsername { get; set; }

}