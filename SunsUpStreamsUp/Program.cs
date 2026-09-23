using NodaTime;
using RuntimeUpgrade.Notifier;
using RuntimeUpgrade.Notifier.Data;
using SunsUpStreamsUp.Logic;
using SunsUpStreamsUp.Options;
using Unfucked.DI;
using Unfucked.HTTP;
using Unfucked.OBS;

Version.PrintProgramVersionAndExitIfRequested();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AlsoSearchForJsonFilesInExecutableDirectory();

builder.Logging.AddUnfuckedConsole(static options => options.Color = true);

builder.Services
    .Configure<StreamOptions>(builder.Configuration.GetSection("stream"))
    .Configure<GeographicOptions>(builder.Configuration.GetSection("geography"))
    .Configure<SocialOptions>(builder.Configuration.GetSection("social"))
    .AddHostedService<StreamManagerImpl>(SuperRegistration.Interfaces)
    .AddHostedService<SolarEventEmitterImpl>(SuperRegistration.Interfaces)
    .AddHostedService<BlueskyClient>()
    .AddSingleton(SystemClock.Instance, SuperRegistration.Interfaces)
    .AddSingleton<ObsClientFactory>(SuperRegistration.Interfaces)
    .AddSingleton<UnfuckedHttpClient>(SuperRegistration.Interfaces)
    .AddSingleton<BlueskyAuthFilter>()
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
} catch (OutOfMemoryException e) {
    Environment.FailFast(e.Message, e);
    return 1;
} catch (Exception) {
    return 1;
}