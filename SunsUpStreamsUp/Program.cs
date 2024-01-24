using NodaTime;
using SunsUpStreamsUp;
using SunsUpStreamsUp.Facades;
using SunsUpStreamsUp.Logic;
using SunsUpStreamsUp.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
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
    .AddSingleton(TimeProvider.System);

using IHost host = builder.Build();
await host.RunAsync();