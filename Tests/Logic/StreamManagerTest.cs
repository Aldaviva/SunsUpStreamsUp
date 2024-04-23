using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OBSStudioClient.Enums;
using OBSStudioClient.Responses;
using SolCalc.Data;
using SunsUpStreamsUp.Facades;
using SunsUpStreamsUp.Options;
using System.ComponentModel;

namespace Tests.Logic;

public class StreamManagerTest: IDisposable {

    private readonly StreamManager           streamManager;
    private readonly SolarEventEmitter       solarEventEmitter = A.Fake<SolarEventEmitter>();
    private readonly IObsClient              obs               = A.Fake<IObsClient>();
    private readonly StreamOptions           options           = new() { obsHostname = "host", obsPassword = "pass", obsPort = 12345 };
    private readonly CancellationTokenSource cts               = new();

    public StreamManagerTest() {
        streamManager = new StreamManager(solarEventEmitter, obs, SystemClock.Instance, new NullLogger<StreamManager>(), new OptionsWrapper<StreamOptions>(options));

        A.CallTo(() => obs.ConnectAsync(A<bool>._, A<string>._, A<string>._, An<int>._, An<EventSubscriptions>._))
            .Invokes(() => obs.PropertyChanged += Raise.FreeForm.With(obs, new PropertyChangedEventArgs("ConnectionState")))
            .Returns(true);

        A.CallTo(() => obs.ConnectionState).Returns(ConnectionState.Connected);

        A.CallTo(() => solarEventEmitter.minimumSunlightLevel).Returns(SunlightLevel.CivilTwilight);
        A.CallTo(() => solarEventEmitter.currentSunlight).Returns(SunlightLevel.Night);
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
        await thrower.Should().ThrowAsync<ObsFailedToConnect>();
    }

    [Theory]
    [InlineData(SunlightLevel.Daylight)]
    [InlineData(SunlightLevel.CivilTwilight)]
    public async Task startStreamOnLaunchIfDaylightAndStreamStopped(SunlightLevel lightLevel) {
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(false, false, string.Empty, 0, 0, 0, 0, 0));
        A.CallTo(() => solarEventEmitter.currentSunlight).Returns(lightLevel);

        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(SunlightLevel.Daylight)]
    [InlineData(SunlightLevel.CivilTwilight)]
    public async Task dontStartStreamOnLaunchIfStreamRunning(SunlightLevel lightLevel) {
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(true, false, string.Empty, 1, 0, 1, 0, 1));
        A.CallTo(() => solarEventEmitter.currentSunlight).Returns(lightLevel);

        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(SunlightLevel.NauticalTwilight)]
    [InlineData(SunlightLevel.AstronomicalTwilight)]
    [InlineData(SunlightLevel.Night)]
    public async Task dontStartStreamOnLaunchIfTooDark(SunlightLevel lightLevel) {
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(false, false, string.Empty, 0, 0, 0, 0, 0));
        A.CallTo(() => solarEventEmitter.currentSunlight).Returns(lightLevel);

        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();
    }

    [Fact]
    public async Task startStreamOnCivilDawnStart() {
        await streamManager.StartAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();

        solarEventEmitter.solarElevationChanged += Raise.With(new SunlightChange(default, SolarTimeOfDay.CivilDawn));

        A.CallTo(() => obs.StartStream()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task stopStreamOnCivilDuskEnd() {
        await streamManager.StartAsync(cts.Token);

        solarEventEmitter.solarElevationChanged += Raise.With(new SunlightChange(default, SolarTimeOfDay.CivilDusk));

        A.CallTo(() => obs.StopStream()).MustHaveHappenedOnceExactly();
    }

}