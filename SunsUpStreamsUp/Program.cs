using Microsoft.Extensions.Options;
using NodaTime;
using SunsUpStreamsUp;
using SunsUpStreamsUp.Logic;
using SunsUpStreamsUp.Options;
using Twitch.Net;
using Unfucked;
using Unfucked.OBS;
using Unfucked.Twitch;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AlsoSearchForJsonFilesInExecutableDirectory();

builder.Logging.AddUnfuckedConsole(options => options.Color = true);

builder.Services
    .Configure<StreamOptions>(builder.Configuration.GetSection("stream"))
    .Configure<GeographicOptions>(builder.Configuration.GetSection("geography"))
    .AddHostedService<StreamManager>()
    .AddSingleton<SolarEventEmitter, SolarEventEmitterImpl>()
    .AddHostedService(s => s.GetRequiredService<SolarEventEmitter>())
    .AddSingleton<IClock>(SystemClock.Instance)
    .AddSingleton<IObsClientFactory, ObsClientFactory>()
    .AddSingleton(TimeProvider.System)
    .AddSingleton<ITwitchApi>(services => {
        StreamOptions options      = services.GetRequiredService<IOptions<StreamOptions>>().Value;
        string?       clientId     = options.twitchClientId;
        string?       clientSecret = options.twitchClientSecret;
        return clientId.HasText() && clientSecret.HasText() ? new TwitchApiClient(new TwitchApiBuilder(clientId).WithClientSecret(clientSecret).Build()) : null!;
        // DI is fine with null return value, even if the .AddSingleton() method isn't annotated as such
    });

using IHost host = builder.Build();

try {
    await host.RunAsync();
    return 0;
} catch (ObsFailedToConnect) {
    return 1;
}