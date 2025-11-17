using NodaTime;
using RuntimeUpgrade.Notifier;
using RuntimeUpgrade.Notifier.Data;
using SunsUpStreamsUp;
using SunsUpStreamsUp.Logic;
using SunsUpStreamsUp.Options;
using Unfucked.DI;
using Unfucked.OBS;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AlsoSearchForJsonFilesInExecutableDirectory();

builder.Logging.AddUnfuckedConsole(options => options.Color = true);

builder.Services
    .Configure<StreamOptions>(builder.Configuration.GetSection("stream"))
    .Configure<GeographicOptions>(builder.Configuration.GetSection("geography"))
    .AddHostedService<StreamManager>()
    .AddHostedService<SolarEventEmitterImpl>(SuperRegistration.INTERFACES)
    .AddSingleton<IClock>(SystemClock.Instance)
    .AddSingleton<IObsClientFactory, ObsClientFactory>()
    .AddSingleton(TimeProvider.System);

using IHost host = builder.Build();

using RuntimeUpgradeNotifier upgradeNotifier = new() {
    LoggerFactory   = host.Services.GetRequiredService<ILoggerFactory>(),
    RestartStrategy = RestartStrategy.AutoRestartProcess,
    ExitStrategy    = new HostedLifetimeExit(host)
};

try {
    await host.RunAsync();
    return 0;
} catch (ObsFailedToConnect) {
    return 1;
}