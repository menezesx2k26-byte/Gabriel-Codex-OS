namespace YouCineBridge;

public sealed class BridgeLifecycle
{
    private readonly Func<CancellationToken, Task> _open;
    private readonly Func<BridgeHotkey, CancellationToken, Task> _execute;
    private readonly Action _stop;

    public BridgeLifecycle(
        Func<CancellationToken, Task> open,
        Func<BridgeHotkey, CancellationToken, Task> execute,
        Action stop)
    {
        _open = open;
        _execute = execute;
        _stop = stop;
    }

    public Task StartAsync(CancellationToken ct) => _open(ct);

    public Task ExecuteAsync(BridgeHotkey hotkey, CancellationToken ct) =>
        _execute(hotkey, ct);

    public void Stop() => _stop();
}
