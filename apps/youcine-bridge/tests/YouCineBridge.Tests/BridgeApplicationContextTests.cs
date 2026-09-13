using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class BridgeApplicationContextTests
{
    [Fact]
    public async Task ExecuteTrayCommandAsync_RoutesMenuActions()
    {
        var calls = new List<string>();
        var lifecycle = new BridgeLifecycle(
            open: _ => { calls.Add("open"); return Task.CompletedTask; },
            execute: (hotkey, _) =>
            {
                calls.Add(hotkey.ToString());
                return Task.CompletedTask;
            },
            stop: () => calls.Add("stop"));

        await BridgeApplicationContext.ExecuteTrayCommandAsync(
            BridgeTrayCommand.Open, lifecycle,
            () => calls.Add("logs"), () => calls.Add("exit"), CancellationToken.None);
        await BridgeApplicationContext.ExecuteTrayCommandAsync(
            BridgeTrayCommand.TogglePip, lifecycle,
            () => calls.Add("logs"), () => calls.Add("exit"), CancellationToken.None);
        await BridgeApplicationContext.ExecuteTrayCommandAsync(
            BridgeTrayCommand.Reconnect, lifecycle,
            () => calls.Add("logs"), () => calls.Add("exit"), CancellationToken.None);
        await BridgeApplicationContext.ExecuteTrayCommandAsync(
            BridgeTrayCommand.OpenLogs, lifecycle,
            () => calls.Add("logs"), () => calls.Add("exit"), CancellationToken.None);
        await BridgeApplicationContext.ExecuteTrayCommandAsync(
            BridgeTrayCommand.Exit, lifecycle,
            () => calls.Add("logs"), () => calls.Add("exit"), CancellationToken.None);

        Assert.Equal(new[]
        {
            "open", "TogglePip", "Reconnect", "logs", "stop", "exit"
        }, calls);
    }

    [Fact]
    public void TrayMenu_ExposesRequiredCommandsInOrder()
    {
        Assert.Equal(new[] { "Open YouCine", "PiP / Fullscreen", "Reconnect", "Open logs", "Exit" },
            BridgeApplicationContext.TrayMenu.Select(item => item.Text));
        Assert.Equal(new[] { BridgeTrayCommand.Open, BridgeTrayCommand.TogglePip,
                BridgeTrayCommand.Reconnect, BridgeTrayCommand.OpenLogs, BridgeTrayCommand.Exit },
            BridgeApplicationContext.TrayMenu.Select(item => item.Command));
    }
}
