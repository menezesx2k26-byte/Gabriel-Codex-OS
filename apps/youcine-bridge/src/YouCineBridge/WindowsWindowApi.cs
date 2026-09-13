using System.Drawing;
using System.Runtime.InteropServices;

namespace YouCineBridge;

public sealed class WindowsWindowApi : IWindowApi
{
    public long GetStyle(IntPtr hwnd) => NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE).ToInt64();
    public long GetExStyle(IntPtr hwnd) => NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();

    public Rectangle GetBounds(IntPtr hwnd)
    {
        if (!NativeMethods.GetWindowRect(hwnd, out var r))
            throw new InvalidOperationException("Could not read YouCine window bounds.");
        return Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
    }

    public Rectangle GetMonitorBounds(IntPtr hwnd)
    {
        var monitor = NativeMethods.MonitorFromWindow(hwnd, 2);
        var info = new NativeMethods.MONITORINFO { cbSize = Marshal.SizeOf<NativeMethods.MONITORINFO>() };
        if (monitor == IntPtr.Zero || !NativeMethods.GetMonitorInfo(monitor, ref info))
            return Screen.PrimaryScreen?.Bounds ?? GetBounds(hwnd);
        var r = info.rcMonitor;
        return Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
    }
    public void SetStyle(IntPtr hwnd, long style) =>
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE, new IntPtr(style));

    public void SetExStyle(IntPtr hwnd, long style) =>
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(style));

    public void SetBounds(IntPtr hwnd, Rectangle bounds, bool topmost, bool frameChanged)
    {
        var after = topmost ? NativeMethods.HWND_TOPMOST : NativeMethods.HWND_NOTOPMOST;
        var flags = frameChanged ? NativeMethods.SWP_FRAMECHANGED : 0u;
        if (!NativeMethods.SetWindowPos(hwnd, after,
            bounds.X, bounds.Y, bounds.Width, bounds.Height, flags))
            throw new InvalidOperationException("Could not move YouCine window.");
    }

    public void Focus(IntPtr hwnd) => NativeMethods.SetForegroundWindow(hwnd);
}
