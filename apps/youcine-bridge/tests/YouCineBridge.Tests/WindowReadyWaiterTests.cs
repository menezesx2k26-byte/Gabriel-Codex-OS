using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class WindowReadyWaiterTests
{
    [Fact]
    public async Task WaitAsync_ReturnsFirstNonZeroHandle()
    {
        var reads = 0;
        var waiter = new WindowReadyWaiter(
            () => ++reads < 3 ? IntPtr.Zero : new IntPtr(77),
            (_, _) => Task.CompletedTask);

        var handle = await waiter.WaitAsync(TimeSpan.FromSeconds(1), CancellationToken.None);

        Assert.Equal(new IntPtr(77), handle);
        Assert.Equal(3, reads);
    }

    [Fact]
    public async Task WaitAsync_ThrowsWhenWindowNeverAppears()
    {
        var waiter = new WindowReadyWaiter(() => IntPtr.Zero, (_, _) => Task.CompletedTask);
        await Assert.ThrowsAsync<TimeoutException>(() =>
            waiter.WaitAsync(TimeSpan.Zero, CancellationToken.None));
    }
}
