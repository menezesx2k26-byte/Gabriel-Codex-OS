using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class BridgeRuntimeCoordinatorTests
{
    [Fact]
    public async Task OpenAsync_ResolvesPersistsAndStartsSession()
    {
        var path = Path.Combine(Path.GetTempPath(), $"youcine-{Guid.NewGuid():N}.json");
        var settings = new BridgeSettings { RuntimeEndpoint = "100.80.1.9:5555" };
        string? started = null;
        var coordinator = new BridgeRuntimeCoordinator(
            settings, path,
            resolve: _ => Task.FromResult(new ResolvedDevice("100.80.1.9:5555", "redroid")),
            ensureRunning: (endpoint, _) => { started = endpoint; return Task.CompletedTask; },
            stop: () => { });

        await coordinator.OpenAsync(CancellationToken.None);

        Assert.True(coordinator.SessionExpected);
        Assert.Equal("100.80.1.9:5555", started);
        Assert.Equal("100.80.1.9:5555", BridgeSettings.Load(path).LastEndpoint);
        File.Delete(path);
    }
    [Fact]
    public void Stop_DisablesExpectedSessionBeforeStoppingRuntime()
    {
        var settings = new BridgeSettings();
        var expectedWhenStopped = true;
        BridgeRuntimeCoordinator? coordinator = null;
        coordinator = new BridgeRuntimeCoordinator(
            settings, Path.GetTempFileName(),
            resolve: _ => throw new NotSupportedException(),
            ensureRunning: (_, _) => throw new NotSupportedException(),
            stop: () => expectedWhenStopped = coordinator!.SessionExpected);

        coordinator.Stop();

        Assert.False(expectedWhenStopped);
        Assert.False(coordinator.SessionExpected);
    }
}
