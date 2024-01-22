using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OBSStudioClient.Responses;
using SunsUpStreamsUp;
using SunsUpStreamsUp.Facades;
using Options = SunsUpStreamsUp.Options;

namespace Tests;

public class SunsUpStreamsUpServiceTest {

    private readonly SunsUpStreamsUpService  service;
    private readonly IObsClient              obs          = A.Fake<IObsClient>();
    private readonly TimeProvider            timeProvider = A.Fake<TimeProvider>(fakeOptions => fakeOptions.Wrapping(TimeProvider.System));
    private readonly CancellationTokenSource cts          = new();

    private readonly Options options = new() {
        latitude  = 37.35489827122742,
        longitude = -121.98216436081027
    };

    public SunsUpStreamsUpServiceTest() {
        service = new SunsUpStreamsUpService(obs, timeProvider, new OptionsWrapper<Options>(options), new NullLogger<SunsUpStreamsUpService>());
    }

    [Fact]
    public void getNextStreamActionBeforeSunrise() {
        DateTime start    = new(2024, 1, 21, 3, 26, 0);
        DateTime expected = new(2024, 1, 21, 6, 50, 42);

        SolarStreamAction actual = service.getNextStreamAction(start);
        actual.shouldStartStream.Should().BeTrue();
        actual.time.Should().BeCloseTo(expected, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void getNextStreamActionBetweenSunriseAndSunset() {
        DateTime start    = new(2024, 1, 21, 12 + 2, 28, 0);
        DateTime expected = new(2024, 1, 21, 12 + 5, 47, 23);

        SolarStreamAction actual = service.getNextStreamAction(start);
        actual.shouldStartStream.Should().BeFalse();
        actual.time.Should().BeCloseTo(expected, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void getNextStreamActionAfterSunset() {
        DateTime start    = new(2024, 1, 21, 12 + 11, 0, 0);
        DateTime expected = new(2024, 1, 22, 6, 50, 0);

        SolarStreamAction actual = service.getNextStreamAction(start);
        actual.time.Should().BeCloseTo(expected, TimeSpan.FromMinutes(1));
        actual.shouldStartStream.Should().BeTrue();
    }

    [Fact]
    public void getNextStreamActionDuringArcticSummer() {
        // Ny-Ålesund, Svalbard, Norway
        options.latitude  = 78.92;
        options.longitude = 11.93;
        DateTime start    = new(2019, 6, 28, 12, 0, 0);
        DateTime expected = new(2019, 9, 10, 15, 45, 3);

        SolarStreamAction actual = service.getNextStreamAction(start);
        actual.time.Should().BeCloseTo(expected, TimeSpan.FromMinutes(1));
        actual.shouldStartStream.Should().BeFalse();
    }

    [Fact]
    public void getNextStreamActionDuringArcticWinter() {
        // Ny-Ålesund, Svalbard, Norway
        options.latitude  = 78.92;
        options.longitude = 11.93;
        DateTime start    = new(2024, 1, 21, 0, 0, 0);
        DateTime expected = new(2024, 2, 2, 2, 46, 0);

        SolarStreamAction actual = service.getNextStreamAction(start);
        actual.time.Should().BeCloseTo(expected, TimeSpan.FromMinutes(1));
        actual.shouldStartStream.Should().BeTrue();
    }

    [Fact]
    public void startStream() {
        service.startOrStopStream(true);
        A.CallTo(() => obs.StartStream()).MustHaveHappened();
        A.CallTo(() => obs.StopStream()).MustNotHaveHappened();
    }

    [Fact]
    public void stopStream() {
        service.startOrStopStream(false);
        A.CallTo(() => obs.StopStream()).MustHaveHappened();
        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();
    }

    [Fact]
    public async Task startStreamAfterDelay() {
        A.CallTo(() => timeProvider.GetUtcNow()).Returns(new DateTimeOffset(2024, 1, 21, 12 + 2, 50, 41, TimeSpan.Zero));
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(false, false, string.Empty, 0, 0, 0, 0, 0));
        TaskCompletionSource streamStarted = new();
        A.CallTo(() => obs.StartStream()).Invokes(streamStarted.SetResult);

        await service.StartAsync(cts.Token);

        await streamStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task startStreamOnStartupIfAlreadyStopped() {
        A.CallTo(() => timeProvider.GetUtcNow()).Returns(new DateTimeOffset(2024, 1, 21, 12 + 2, 50, 43, TimeSpan.Zero));
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(false, false, string.Empty, 0, 0, 0, 0, 0));
        TaskCompletionSource streamStarted = new();
        A.CallTo(() => obs.StartStream()).Invokes(streamStarted.SetResult);

        await service.StartAsync(cts.Token);

        await streamStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task dontStartStreamOnStartupIfAlreadyStarted() {
        A.CallTo(() => timeProvider.GetUtcNow()).Returns(new DateTimeOffset(2024, 1, 21, 12 + 2, 50, 43, TimeSpan.Zero));
        A.CallTo(() => obs.GetStreamStatus()).Returns(new OutputStatusResponse(true, false, string.Empty, 0, 0, 0, 0, 0));

        await service.StartAsync(cts.Token);

        await Task.Delay(TimeSpan.FromSeconds(2));
        await service.StopAsync(cts.Token);

        A.CallTo(() => obs.StartStream()).MustNotHaveHappened();
    }

}