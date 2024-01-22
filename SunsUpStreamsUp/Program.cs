using Microsoft.Extensions.Options;
using OBSStudioClient.Enums;
using SunsUpStreamsUp;
using SunsUpStreamsUp.Facades;
using System.Net.WebSockets;
using Options = SunsUpStreamsUp.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services
    .Configure<Options>(builder.Configuration)
    .AddHostedService<Startup>()
    .AddHostedService<SunsUpStreamsUpService>()
    .AddSingleton<IObsClient, ObsClientFacade>()
    .AddSingleton(TimeProvider.System);
builder.Logging.AddSimpleConsole(options => options.TimestampFormat = "yyyy-MM-dd h:mm:ss tt: ");

using IHost host = builder.Build();
await host.RunAsync();

internal class Startup(
    IObsClient        obs,
    IOptions<Options> options,
    ILogger<Startup>  logger
): IHostedService {

    /// <exception cref="WebSocketException">if the connection or authentication fail</exception>
    public async Task StartAsync(CancellationToken cancellationToken) {
        TaskCompletionSource authenticated = new();
        obs.PropertyChanged += (_, eventArgs) => {
            if (eventArgs.PropertyName == nameof(IObsClient.ConnectionState) && obs.ConnectionState == ConnectionState.Connected) {
                authenticated.SetResult();
            }
        };

        logger.LogDebug("Connecting to OBS at ws://{host}:{port}", options.Value.obsHostname, options.Value.obsPort);
        if (!await obs.ConnectAsync(true, options.Value.obsPassword, options.Value.obsHostname, options.Value.obsPort, EventSubscriptions.None)) {
            throw new WebSocketException($"Failed to connect to OBS at {options.Value.obsHostname}:{options.Value.obsPort}");
        }

        await authenticated.Task.WaitAsync(cancellationToken);
        logger.LogInformation("Connected to OBS");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}