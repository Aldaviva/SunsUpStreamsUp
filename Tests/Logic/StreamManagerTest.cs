using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OBSStudioClient.Enums;
using OBSStudioClient.Responses;
using SunsUpStreamsUp.Facades;
using SunsUpStreamsUp.Options;
using System.ComponentModel;
using System.Net.WebSockets;

namespace Tests.Logic;

public class StreamManagerTest: IDisposable {

    private readonly StreamManager           streamManager;
    private readonly SolarEventEmitter       solarEventEmitter = A.Fake<SolarEventEmitter>();
    private readonly IObsClient              obs               = A.Fake<IObsClient>();
    private readonly StreamOptions           options           = new() { obsHostname = "host", obsPassword = "pass", obsPort = 12345 };
    private readonly CancellationTokenSource cts               = new();

    public StreamManagerTest() {
        streamManager = new StreamManager(solarEventEmitter, obs, new OptionsWrapper<StreamOptions>(options), new NullLogger<StreamManager>());

        A.CallTo(() => obs.ConnectAsync(A<bool>._, A<string>._, A<string>._, An<int>._, An<EventSubscriptions>._))
            .Invokes(() => obs.PropertyChanged += Raise.FreeForm.With(obs, new PropertyChangedEventArgs("ConnectionState")))
            .Returns(true);

        A.CallTo(() => obs.ConnectionState).Returns(ConnectionState.Connected);

        A.CallTo(() => solarEventEmitter.currentSunlight).Returns(Sunlight.NIGHT);
    }

    public void Dispose() {
        streamManager.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task authSucceeded() {
        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.ConnectAsync(true, "pass", "host", 12345, EventSubscriptions.None)).MustHaveHappened();
    }

    [Fact]
    public async Task authFailed() {
        A.CallTo(() => obs.ConnectAsync(A<bool>._, A<string>._, A<string>._, An<int>._, An<EventSubscriptions>._)).Returns(false);
        A.CallTo(() => obs.ConnectionState).Returns(ConnectionState.Disconnected);

        Func<Task> thrower = async () => await streamManager.StartAsync(cts.Token);
        await thrower.Should().ThrowAsync<WebSocketException>();
    }

    [Theory]
    [InlineData(Sunlight.DAYLIGHT)]
    [InlineData(Sunlight.CIVIL_TWILIGHT)]
    public async Task startStreamOnLaunchIfDaylightAndStreamStopped(Sunlight lightLevel) {
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(false, false, string.Empty, 0, 0, 0, 0, 0));
        A.CallTo(() => solarEventEmitter.currentSunlight).Returns(lightLevel);

        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(Sunlight.DAYLIGHT)]
    [InlineData(Sunlight.CIVIL_TWILIGHT)]
    public async Task dontStartStreamOnLaunchIfStreamRunning(Sunlight lightLevel) {
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(true, false, string.Empty, 1, 0, 1, 0, 1));
        A.CallTo(() => solarEventEmitter.currentSunlight).Returns(lightLevel);

        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(Sunlight.NAUTICAL_TWILIGHT)]
    [InlineData(Sunlight.ASTRONOMICAL_TWILIGHT)]
    [InlineData(Sunlight.NIGHT)]
    public async Task dontStartStreamOnLaunchIfTooDark(Sunlight lightLevel) {
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(false, false, string.Empty, 0, 0, 0, 0, 0));
        A.CallTo(() => solarEventEmitter.currentSunlight).Returns(lightLevel);

        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();
    }

    [Fact]
    public async Task startStreamOnCivilDawnStart() {
        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();

        solarEventEmitter.solarElevationChanged += Raise.With(new SolarElevationChange(default, Sunlight.NAUTICAL_TWILIGHT, Sunlight.CIVIL_TWILIGHT, true));

        A.CallTo(() => obs.StartStream()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task stopStreamOnCivilDuskEnd() {
        await streamManager.StartAsync(cts.Token);

        solarEventEmitter.solarElevationChanged += Raise.With(new SolarElevationChange(default, Sunlight.CIVIL_TWILIGHT, Sunlight.NAUTICAL_TWILIGHT, false));

        A.CallTo(() => obs.StopStream()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task dontModifyStreamOnOtherSolarElevationChanges() {
        await streamManager.StartAsync(cts.Token);

        solarEventEmitter.solarElevationChanged += Raise.With(new SolarElevationChange(default, Sunlight.NIGHT, Sunlight.ASTRONOMICAL_TWILIGHT, true));
        solarEventEmitter.solarElevationChanged += Raise.With(new SolarElevationChange(default, Sunlight.ASTRONOMICAL_TWILIGHT, Sunlight.NAUTICAL_TWILIGHT, true));
        solarEventEmitter.solarElevationChanged += Raise.With(new SolarElevationChange(default, Sunlight.CIVIL_TWILIGHT, Sunlight.DAYLIGHT, true));

        solarEventEmitter.solarElevationChanged += Raise.With(new SolarElevationChange(default, Sunlight.DAYLIGHT, Sunlight.CIVIL_TWILIGHT, false));
        solarEventEmitter.solarElevationChanged += Raise.With(new SolarElevationChange(default, Sunlight.NAUTICAL_TWILIGHT, Sunlight.ASTRONOMICAL_TWILIGHT, false));
        solarEventEmitter.solarElevationChanged += Raise.With(new SolarElevationChange(default, Sunlight.ASTRONOMICAL_TWILIGHT, Sunlight.NIGHT, false));

        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();
        A.CallTo(() => obs.StopStream()).MustNotHaveHappened();
    }

}