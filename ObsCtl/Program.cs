using McMaster.Extensions.CommandLineUtils;
using OBSStudioClient.Classes;
using OBSStudioClient.Enums;
using OBSStudioClient.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Unfucked.OBS;

namespace ObsCtl;

[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")] // constructed by CommandLineApplication
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")] // assigned by CommandLineApplication
internal class Program {

    [Argument(0, "command", "Action for the program to take.")]
    [Required]
    public Command? command { get; }

    private static readonly Lazy<ObsClientFactory> OBS_CLIENT_FACTORY_PROVIDER = new(LazyThreadSafetyMode.PublicationOnly);
    private static readonly TimeSpan               TIMEOUT                     = TimeSpan.FromSeconds(5);

    private static CancellationToken cancellationToken;

    public static async Task<int> Main(string[] args) {
        CancellationTokenSource cts = new();
        cancellationToken = cts.Token;
        Console.CancelKeyPress += (_, eventArgs) => {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        var app = new CommandLineApplication<Program> {
            UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Throw,
            Description                  = "Control OBS (Open Broadcaster Software Studio) from the command line"
        };
        app.Conventions.UseDefaultConventions();
        app.ExtendedHelpText =
            $"""

             Examples:
               Show status of stream, which will print either "live", "idle", or "exited":
                 {app.Name} status
                 
               Start stream, if OBS is already running:
                 {app.Name} start
             
               Stop stream, after which OBS will continue running:
                 {app.Name} stop
                 
             Prerequisites:
               • OBS must already be running if you want to start the stream — this tool won't launch OBS.
               • The OBS WebSocket server must be enabled using Tools › WebSocket Server Settings › Enable WebSocket Server.
               • When using a non-default host or password, they must be specified in the environment variables below.

             Environment variables:
               OBS_HOST
                 A URI containing the hostname and port of the OBS WebSocket server. Defaults to ws://localhost:4455.
                 You can change this using Tools › WebSocket Server Settings › Server Port.
                 
               OBS_PASSWORD
                 The password to the OBS WebSocket server. Not URL-encoded. Defaults to the empty string for disabled authentication.
                 You can change this using Tools › WebSocket Server Settings › Enable Authentication and Server Password.
             """;

        try {
            return await app.ExecuteAsync(args, cts.Token);
        } catch (TaskCanceledException) {
            return 1;
        } catch (CommandParsingException e) {
            Console.WriteLine($"Error: {e.Message}");
            Console.WriteLine($"For usage information, run `{app.Name} --help`");
            return 2;
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")] // called by CommandLineApplication
    private async Task<int> OnExecute() {
        Uri?   obsHost     = Environment.GetEnvironmentVariable("OBS_HOST") is { } host ? new Uri(host) : null;
        string obsPassword = Environment.GetEnvironmentVariable("OBS_PASSWORD") ?? string.Empty;

        using IObsClient? obs = await OBS_CLIENT_FACTORY_PROVIDER.Value.Connect(obsHost, obsPassword,
            CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource(TIMEOUT).Token, cancellationToken).Token);

        bool streamWasLive = obs != null && (await obs.GetStreamStatus().WaitAsync(TIMEOUT, cancellationToken)).OutputActive;

        switch (command) {
            case Command.start:
                if (obs is null) {
                    Console.WriteLine("OBS is not running, please launch it before starting the stream.");
                    return 1;
                }
                // } else if (!streamWasLive) {
                await obs.StartStream().WaitAsync(TIMEOUT, cancellationToken);
                // }
                break;
            case Command.stop:
                if (streamWasLive && obs != null) {
                    try {
                        await obs.StopStream().WaitAsync(TIMEOUT, cancellationToken);
                    } catch (ObsResponseException e) when (e.ErrorCode == RequestStatusCode.OutputNotRunning) {
                        Console.WriteLine("Stream stuck, stopping all outputs");
                        Output[] allOutputs = await obs.GetOutputList().WaitAsync(TIMEOUT, cancellationToken);
                        foreach (Output output in allOutputs.Where(output => output.OutputActive)) {
                            await obs.StopOutput(output.OutputName).WaitAsync(TIMEOUT, cancellationToken);
                            Console.WriteLine($"Stopped output {output.OutputName}");
                        }
                        await obs.StopStream().WaitAsync(TIMEOUT, cancellationToken);
                    }
                }
                break;
            case Command.status:
                Console.WriteLine(obs == null ? "exited" : streamWasLive ? "live" : "idle");
                break;
        }

        return 0;
    }

}