using Microsoft.Extensions.Options;
using NodaTime;
using SunsUpStreamsUp;
using SunsUpStreamsUp.Facades;
using SunsUpStreamsUp.Logic;
using SunsUpStreamsUp.Options;
using Twitch.Net;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AlsoSearchForJsonFilesInExecutableDirectory();

builder.Logging.AddConsole(options => options.FormatterName = MyConsoleFormatter.NAME).AddConsoleFormatter<MyConsoleFormatter, MyConsoleFormatter.MyConsoleOptions>(options => {
    options.includeNamespaces = false;
});

builder.Services
    .Configure<StreamOptions>(builder.Configuration.GetSection("stream"))
    .Configure<GeographicOptions>(builder.Configuration.GetSection("geography"))
    .AddHostedService<StreamManager>()
    .AddSingleton<SolarEventEmitter, SolarEventEmitterImpl>()
    .AddHostedService(s => s.GetRequiredService<SolarEventEmitter>())
    .AddSingleton<IClock>(SystemClock.Instance)
    .AddSingleton<IObsClient, ObsClientFacade>()
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
    host.Run();
    return 0;
} catch (ObsFailedToConnect) {
    return 1;
}