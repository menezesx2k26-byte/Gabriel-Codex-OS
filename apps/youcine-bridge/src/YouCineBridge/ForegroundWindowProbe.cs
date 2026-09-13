using System.Runtime.InteropServices;

namespace YouCineBridge;

public static class ForegroundWindowProbe
{
    public static bool IsForeground(IntPtr hwnd) =>
        hwnd != IntPtr.Zero && GetForegroundWindow() == hwnd;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
}
