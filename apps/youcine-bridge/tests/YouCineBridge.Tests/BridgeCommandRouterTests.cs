using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class BridgeCommandRouterTests
{
    [Fact]
    public async Task ExecuteAsync_RoutesAllBridgeCommands()
    {
        var calls = new List<string>();
        var router = new BridgeCommandRouter(
            togglePip: () => calls.Add("pip"),
            focus: () => calls.Add("focus"),
            reconnect: _ => { calls.Add("reconnect"); return Task.CompletedTask; },
            toggleFullscreen: () => calls.Add("fullscreen"));

        await router.ExecuteAsync(BridgeHotkey.TogglePip, CancellationToken.None);
        await router.ExecuteAsync(BridgeHotkey.Focus, CancellationToken.None);
        await router.ExecuteAsync(BridgeHotkey.Reconnect, CancellationToken.None);
        await router.ExecuteAsync(BridgeHotkey.ToggleFullscreen, CancellationToken.None);

        Assert.Equal(new[] { "pip", "focus", "reconnect", "fullscreen" }, calls);
    }
}
