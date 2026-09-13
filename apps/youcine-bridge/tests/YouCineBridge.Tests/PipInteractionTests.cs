using System.Drawing;
using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class PipInteractionTests
{
    private static readonly Rectangle Bounds = new(100, 100, 640, 360);

    [Theory]
    [InlineData(110, 105, false, true)]
    [InlineData(300, 220, true, true)]
    [InlineData(300, 220, false, false)]
    [InlineData(50, 50, true, false)]
    public void ShouldStartMove_OnlyUsesGrabStripOrAltInsideWindow(
        int x, int y, bool altHeld, bool expected)
    {
        Assert.Equal(expected,
            PipInteractionHook.ShouldStartMove(new Point(x, y), Bounds, altHeld, 12));
    }

    [Fact]
    public void NormalizeAndPersist_StoresClampedSixteenByNineBounds()
    {
        var settings = new BridgeSettings();
        var normalized = PipInteractionHook.NormalizeBounds(
            new Rectangle(1200, 700, 480, 330), new Rectangle(0, 0, 1280, 720));

        Assert.Equal(new Rectangle(800, 450, 480, 270), normalized);
        settings.PipBounds = normalized;
        Assert.Equal(normalized, settings.PipBounds);
    }
}

public sealed class PipResizeCompletionTests
{
    [Theory]
    [InlineData(480, 270, false)]
    [InlineData(480, 330, true)]
    [InlineData(640, 361, false)]
    public void NeedsNormalization_OnlyFlagsMeaningfulAspectDrift(int width, int height, bool expected)
    {
        Assert.Equal(expected, PipInteractionHook.NeedsNormalization(new Rectangle(0, 0, width, height)));
    }
}
