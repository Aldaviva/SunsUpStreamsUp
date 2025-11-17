using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OBSStudioClient.Enums;
using OBSStudioClient.Exceptions;
using OBSStudioClient.Responses;
using SolCalc.Data;
using SunsUpStreamsUp.Options;
using System.ComponentModel;
using Unfucked.OBS;

namespace Tests.Logic;

public class StreamManagerTest: IDisposable {

    private readonly StreamManager           streamManager;
    private readonly SolarEventEmitter       solarEventEmitter = A.Fake<SolarEventEmitter>();
    private readonly IObsClientFactory       obsFactory        = A.Fake<IObsClientFactory>();
    private readonly IObsClient              obs               = A.Fake<IObsClient>();
    private readonly StreamOptions           options           = new() { obsHostname = "host", obsPassword = "pass", obsPort = 12345 };
    private readonly CancellationTokenSource cts               = new();

    public StreamManagerTest() {
        streamManager = new StreamManager(solarEventEmitter, obsFactory, SystemClock.Instance, new NullLogger<StreamManager>(), new OptionsWrapper<StreamOptions>(options));

        A.CallTo(() => obsFactory.Connect(A<Uri>._, A<string>._, A<CancellationToken>._)).Returns(obs);

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

        A.CallTo(() => obsFactory.Connect(new Uri("ws://host:12345"), "pass", A<CancellationToken>._)).MustHaveHappened();
    }

    [Fact]
    public async Task authFailed() {
        A.CallTo(() => obsFactory.Connect(A<Uri>._, A<string>._, A<CancellationToken>._))
            .Returns<IObsClient?>(null).Once().Then.Throws<TestException>();

        Func<Task> thrower = async () => await streamManager.StartAsync(cts.Token);
        await thrower.Should().ThrowAsync<TestException>();
    }

    private class TestException: Exception;

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

    [Fact]
    public async Task dontCrashIfStoppingWhileAlreadyStopped() {
        A.CallTo(() => obs.StopStream()).ThrowsAsync(new ObsResponseException(RequestStatusCode.OutputNotRunning, null));

        await streamManager.StartAsync(cts.Token);

        solarEventEmitter.solarElevationChanged += Raise.With(new SunlightChange(default, SolarTimeOfDay.CivilDawn));
        solarEventEmitter.solarElevationChanged += Raise.With(new SunlightChange(default, SolarTimeOfDay.CivilDusk));
    }

}