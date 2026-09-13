using System.Drawing;

namespace YouCineBridge;

public static class PipGeometry
{
    public const int DefaultWidth = 480;
    public const int Margin = 16;

    public static Rectangle DefaultFor(Rectangle workArea)
    {
        var width = Math.Min(DefaultWidth, Math.Max(16, workArea.Width - 2 * Margin));
        var height = (int)Math.Round(width * 9d / 16d);
        if (height > workArea.Height - 2 * Margin)
        {
            height = Math.Max(9, workArea.Height - 2 * Margin);
            width = (int)Math.Round(height * 16d / 9d);
        }

        return new Rectangle(
            workArea.Right - Margin - width,
            workArea.Bottom - Margin - height,
            width,
            height);
    }

    public static Rectangle Normalize16By9(Rectangle requested, Rectangle workArea)
    {
        var width = Math.Clamp(requested.Width, 160, Math.Max(160, workArea.Width));
        var height = (int)Math.Round(width * 9d / 16d);
        if (height > workArea.Height)
        {
            height = workArea.Height;
            width = (int)Math.Round(height * 16d / 9d);
        }

        var x = Math.Clamp(requested.X, workArea.Left, Math.Max(workArea.Left, workArea.Right - width));
        var y = Math.Clamp(requested.Y, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - height));
        return new Rectangle(x, y, width, height);
    }
}
