using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using NodaTime;
using SunsUpStreamsUp;
using SunsUpStreamsUp.Facades;
using SunsUpStreamsUp.Logic;
using SunsUpStreamsUp.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

/*
 * By default, the .NET host only looks for configuration files in the working directory, not the installation directory, which breaks when you run the program from any other directory.
 * Fix this by also looking for JSON configuration files in the same directory as this executable.
 */
if (Path.GetDirectoryName(Environment.ProcessPath) is { } installationDir && new PhysicalFileProvider(installationDir) is var fileProvider && builder.Configuration.Sources is var sources) {
    int insertAt = sources.OfType<JsonConfigurationSource>().FirstOrDefault() is { } existingJsonSource ? sources.IndexOf(existingJsonSource) : sources.Count;
    sources.Insert(insertAt, new JsonConfigurationSource { FileProvider = fileProvider, Optional = true, ReloadOnChange = true, Path = $"appsettings.{builder.Environment.EnvironmentName}.json" });
    sources.Insert(insertAt, new JsonConfigurationSource { FileProvider = fileProvider, Optional = true, ReloadOnChange = true, Path = "appsettings.json" });
}

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

try {
    host.Run();
    return 0;
} catch (ObsFailedToConnect) {
    return 1;
}