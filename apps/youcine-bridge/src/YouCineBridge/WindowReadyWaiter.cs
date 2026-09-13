namespace YouCineBridge;

public sealed class WindowReadyWaiter
{
    private readonly Func<IntPtr> _handleProvider;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;

    public WindowReadyWaiter(
        Func<IntPtr> handleProvider,
        Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        _handleProvider = handleProvider;
        _delay = delay ?? Task.Delay;
    }

    public async Task<IntPtr> WaitAsync(TimeSpan timeout, CancellationToken ct)
    {
        var started = DateTime.UtcNow;
        do
        {
            var handle = _handleProvider();
            if (handle != IntPtr.Zero) return handle;
            if (DateTime.UtcNow - started >= timeout) break;
            await _delay(TimeSpan.FromMilliseconds(100), ct);
        } while (true);

        throw new TimeoutException("YouCine window did not become ready in time.");
    }
}
