namespace YouCineBridge;

public sealed class RetryPolicy
{
    private readonly TimeSpan _initial;
    private readonly TimeSpan _maximum;
    private int _attempt;

    public RetryPolicy(TimeSpan initial, TimeSpan maximum)
    {
        _initial = initial;
        _maximum = maximum;
    }

    public TimeSpan NextDelay()
    {
        var factor = Math.Pow(2, _attempt++);
        var milliseconds = Math.Min(_initial.TotalMilliseconds * factor, _maximum.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    public void Reset() => _attempt = 0;
}
