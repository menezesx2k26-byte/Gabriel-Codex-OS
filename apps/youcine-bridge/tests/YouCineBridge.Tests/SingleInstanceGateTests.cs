using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class SingleInstanceGateTests
{
    [Fact]
    public void SecondaryInstance_SignalsPrimaryInsteadOfBecomingPrimary()
    {
        var name = "YouCineBridge.Tests." + Guid.NewGuid().ToString("N");
        using var primary = new SingleInstanceGate(name);
        using var activated = new ManualResetEventSlim(false);
        primary.ActivationRequested += (_, _) => activated.Set();

        using var secondary = new SingleInstanceGate(name);
        Assert.True(primary.IsPrimary);
        Assert.False(secondary.IsPrimary);

        secondary.SignalPrimary();
        Assert.True(activated.Wait(TimeSpan.FromSeconds(2)));
    }
}
