namespace YouCineBridge;

public sealed class BridgeRuntimeCoordinator
{
    private readonly BridgeSettings _settings;
    private readonly string _settingsPath;
    private readonly Func<CancellationToken, Task<ResolvedDevice>> _resolve;
    private readonly Func<string, CancellationToken, Task> _ensureRunning;
    private readonly Action _stop;

    public BridgeRuntimeCoordinator(
        BridgeSettings settings,
        string settingsPath,
        Func<CancellationToken, Task<ResolvedDevice>> resolve,
        Func<string, CancellationToken, Task> ensureRunning,
        Action stop)
    {
        _settings = settings;
        _settingsPath = settingsPath;
        _resolve = resolve;
        _ensureRunning = ensureRunning;
        _stop = stop;
    }

    public bool SessionExpected { get; private set; }

    public async Task OpenAsync(CancellationToken ct)
    {
        SessionExpected = true;
        var device = await _resolve(ct);
        _settings.LastEndpoint = device.Endpoint;
        _settings.Save(_settingsPath);
        await _ensureRunning(device.Endpoint, ct);
    }

    public async Task ReconnectAsync(CancellationToken ct)
    {
        _stop();
        await OpenAsync(ct);
    }

    public void Stop()
    {
        SessionExpected = false;
        _stop();
    }
}
