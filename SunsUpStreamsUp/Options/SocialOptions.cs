namespace SunsUpStreamsUp.Options;

public sealed record SocialOptions {

    public string? blueskyUsername { get; init; }
    public string? blueskyPassword { get; init; }
    public string? twitchUsername { get; init; }

}