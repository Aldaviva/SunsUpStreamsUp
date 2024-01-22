using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OBSStudioClient.Enums;
using SunsUpStreamsUp.Facades;
using System.ComponentModel;
using System.Net.WebSockets;
using Options = SunsUpStreamsUp.Options;

namespace Tests;

public class StartupTest {

    private readonly Startup                 startup;
    private readonly IObsClient              obs = A.Fake<IObsClient>();
    private readonly CancellationTokenSource cts = new();

    private readonly Options options = new() {
        obsPort     = 4456,
        obsHostname = "tyr.aldaviva.com",
        obsPassword = "password"
    };

    public StartupTest() {
        startup = new Startup(obs, new OptionsWrapper<Options>(options), new NullLogger<Startup>());
    }

    [Fact]
    public async Task authSucceeded() {
        A.CallTo(() => obs.ConnectAsync(A<bool>._, A<string>._, A<string>._, An<int>._, An<EventSubscriptions>._)).Returns(true);
        A.CallTo(() => obs.ConnectionState).Returns(ConnectionState.Connected);

        Task startTask = startup.StartAsync(cts.Token);
        obs.PropertyChanged += Raise.FreeForm.With(obs, new PropertyChangedEventArgs("ConnectionState"));
        await startTask;

        A.CallTo(() => obs.ConnectAsync(true, "password", "tyr.aldaviva.com", 4456, EventSubscriptions.None)).MustHaveHappened();
    }

    [Fact]
    public async Task authFailed() {
        A.CallTo(() => obs.ConnectAsync(A<bool>._, A<string>._, A<string>._, An<int>._, An<EventSubscriptions>._)).Returns(false);

        Func<Task> thrower = async () => await startup.StartAsync(cts.Token);
        await thrower.Should().ThrowAsync<WebSocketException>();
    }

    [Fact]
    public async Task stopDoesNothing() {
        await startup.StopAsync(cts.Token);
    }

}