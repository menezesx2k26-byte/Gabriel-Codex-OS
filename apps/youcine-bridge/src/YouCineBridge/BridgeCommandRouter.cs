namespace YouCineBridge;

public sealed class BridgeCommandRouter
{
    private readonly Action _togglePip;
    private readonly Action _focus;
    private readonly Func<CancellationToken, Task> _reconnect;
    private readonly Action _toggleFullscreen;

    public BridgeCommandRouter(
        Action togglePip,
        Action focus,
        Func<CancellationToken, Task> reconnect,
        Action toggleFullscreen)
    {
        _togglePip = togglePip;
        _focus = focus;
        _reconnect = reconnect;
        _toggleFullscreen = toggleFullscreen;
    }

    public Task ExecuteAsync(BridgeHotkey hotkey, CancellationToken ct)
    {
        switch (hotkey)
        {
            case BridgeHotkey.TogglePip: _togglePip(); return Task.CompletedTask;
            case BridgeHotkey.Focus: _focus(); return Task.CompletedTask;
            case BridgeHotkey.Reconnect: return _reconnect(ct);
            case BridgeHotkey.ToggleFullscreen: _toggleFullscreen(); return Task.CompletedTask;
            default: throw new ArgumentOutOfRangeException(nameof(hotkey), hotkey, null);
        }
    }
}
