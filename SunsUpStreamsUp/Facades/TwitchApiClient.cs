using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using Twitch.Net;
using Twitch.Net.Interfaces;

namespace SunsUpStreamsUp.Facades;

[ExcludeFromCodeCoverage]
[GeneratedCode("TwitchApi.Net", "3.2.0")]
public class TwitchApiClient(TwitchApi client) : ITwitchApi {

    private readonly TwitchApi client = client;

    public IClipActions Clips => client.Clips;
    public IGameActions Games => client.Games;
    public IStreamActions Streams => client.Streams;
    public IUserActions Users => client.Users;
    public IVideoActions Videos => client.Videos;

}