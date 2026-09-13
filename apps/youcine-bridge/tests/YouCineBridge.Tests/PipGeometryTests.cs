using System.Drawing;
using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class PipGeometryTests
{
    [Fact]
    public void Normalize16By9_Converts480By300To480By270()
    {
        var work = new Rectangle(0, 0, 1280, 680);
        var input = new Rectangle(100, 100, 480, 300);

        var result = PipGeometry.Normalize16By9(input, work);

        Assert.Equal(new Rectangle(100, 100, 480, 270), result);
    }

    [Fact]
    public void DefaultFor_UsesBottomRightWith16PixelMargin()
    {
        var work = new Rectangle(0, 0, 1280, 680);

        var result = PipGeometry.DefaultFor(work);

        Assert.Equal(new Rectangle(784, 394, 480, 270), result);
    }
}
