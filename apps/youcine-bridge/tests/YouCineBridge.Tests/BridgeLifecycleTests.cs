using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class BridgeLifecycleTests
{
    [Fact]
    public async Task Lifecycle_StartsRoutesAndStopsInOrder()
    {
        var calls = new List<string>();
        var lifecycle = new BridgeLifecycle(
            open: _ => { calls.Add("open"); return Task.CompletedTask; },
            execute: (h, _) => { calls.Add(h.ToString()); return Task.CompletedTask; },
            stop: () => calls.Add("stop"));

        await lifecycle.StartAsync(CancellationToken.None);
        await lifecycle.ExecuteAsync(BridgeHotkey.TogglePip, CancellationToken.None);
        lifecycle.Stop();

        Assert.Equal(new[] { "open", "TogglePip", "stop" }, calls);
    }
}
