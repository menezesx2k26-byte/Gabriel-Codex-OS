namespace YouCineBridge;

public sealed class SessionWatchdog
{
    private readonly Func<bool> _shouldRun;
    private readonly Func<bool> _isAlive;
    private readonly Func<CancellationToken, Task> _recover;
    private readonly RetryPolicy _retryPolicy;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;

    public SessionWatchdog(
        Func<bool> shouldRun,
        Func<bool> isAlive,
        Func<CancellationToken, Task> recover,
        RetryPolicy retryPolicy,
        Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        _shouldRun = shouldRun;
        _isAlive = isAlive;
        _recover = recover;
        _retryPolicy = retryPolicy;
        _delay = delay ?? Task.Delay;
    }

    public async Task CheckOnceAsync(CancellationToken ct)
    {
        if (!_shouldRun()) return;
        if (_isAlive())
        {
            _retryPolicy.Reset();
            return;
        }
        var backoff = _retryPolicy.NextDelay();
        if (backoff > TimeSpan.Zero) await _delay(backoff, ct);
        if (!_shouldRun() || _isAlive()) return;

        await _recover(ct);
        if (_isAlive()) _retryPolicy.Reset();
    }

    public async Task RunAsync(TimeSpan interval, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await CheckOnceAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                // Keep the watchdog alive; retry policy controls subsequent recovery pacing.
            }

            try
            {
                await _delay(interval, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
