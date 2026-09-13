using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class SessionWatchdogTests
{
    [Fact]
    public async Task CheckOnceAsync_RecoversDeadExpectedSession()
    {
        var alive = false;
        var recoveries = 0;
        var watchdog = new SessionWatchdog(
            shouldRun: () => true,
            isAlive: () => alive,
            recover: _ => { recoveries++; alive = true; return Task.CompletedTask; },
            retryPolicy: new RetryPolicy(TimeSpan.Zero, TimeSpan.Zero),
            delay: static (_, _) => Task.CompletedTask);

        await watchdog.CheckOnceAsync(CancellationToken.None);

        Assert.Equal(1, recoveries);
        Assert.True(alive);
    }

    [Fact]
    public async Task CheckOnceAsync_DoesNothingWhenSessionIsNotExpected()
    {
        var recoveries = 0;
        var watchdog = new SessionWatchdog(
            shouldRun: () => false,
            isAlive: () => false,
            recover: _ => { recoveries++; return Task.CompletedTask; },
            retryPolicy: new RetryPolicy(TimeSpan.Zero, TimeSpan.Zero),
            delay: static (_, _) => Task.CompletedTask);

        await watchdog.CheckOnceAsync(CancellationToken.None);
        Assert.Equal(0, recoveries);
    }
}
