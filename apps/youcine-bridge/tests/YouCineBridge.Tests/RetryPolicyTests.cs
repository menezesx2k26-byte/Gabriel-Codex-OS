using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class RetryPolicyTests
{
    [Fact]
    public void NextDelay_UsesBoundedExponentialBackoff()
    {
        var policy = new RetryPolicy(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(8));

        Assert.Equal(TimeSpan.FromSeconds(1), policy.NextDelay());
        Assert.Equal(TimeSpan.FromSeconds(2), policy.NextDelay());
        Assert.Equal(TimeSpan.FromSeconds(4), policy.NextDelay());
        Assert.Equal(TimeSpan.FromSeconds(8), policy.NextDelay());
        Assert.Equal(TimeSpan.FromSeconds(8), policy.NextDelay());
    }

    [Fact]
    public void Reset_StartsBackAtInitialDelay()
    {
        var policy = new RetryPolicy(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(8));
        _ = policy.NextDelay();
        _ = policy.NextDelay();
        policy.Reset();

        Assert.Equal(TimeSpan.FromSeconds(1), policy.NextDelay());
    }
}
