namespace YouCineBridge;

public sealed class SingleInstanceGate : IDisposable
{
    private readonly Mutex _mutex;
    private readonly EventWaitHandle _activation;
    private readonly RegisteredWaitHandle? _registration;
    private bool _disposed;

    public SingleInstanceGate(string baseName)
    {
        var safeName = baseName.Replace('\\', '.').Replace('/', '.');
        _mutex = new Mutex(initiallyOwned: true, $"Local\\{safeName}.Mutex", out var createdNew);
        IsPrimary = createdNew;
        _activation = new EventWaitHandle(false, EventResetMode.AutoReset, $"Local\\{safeName}.Activate");
        if (IsPrimary)
        {
            _registration = ThreadPool.RegisterWaitForSingleObject(
                _activation,
                static (state, _) => ((SingleInstanceGate)state!).OnActivation(),
                this,
                Timeout.Infinite,
                executeOnlyOnce: false);
        }
    }

    public bool IsPrimary { get; }
    public event EventHandler? ActivationRequested;

    public void SignalPrimary() => _activation.Set();

    private void OnActivation() => ActivationRequested?.Invoke(this, EventArgs.Empty);
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _registration?.Unregister(null);
        _activation.Dispose();
        if (IsPrimary)
        {
            try { _mutex.ReleaseMutex(); }
            catch (ApplicationException) { }
        }
        _mutex.Dispose();
    }
}
