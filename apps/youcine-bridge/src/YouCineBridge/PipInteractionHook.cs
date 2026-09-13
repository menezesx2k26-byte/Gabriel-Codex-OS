using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace YouCineBridge;

public sealed class PipInteractionHook : IDisposable
{
    private const int WhMouseLl = 14;
    private const int WmMouseMove = 0x0200;
    private const int WmLButtonDown = 0x0201;
    private const int WmLButtonUp = 0x0202;
    private const int VkMenu = 0x12;
    private readonly WindowController _window;
    private readonly Func<IntPtr> _handleProvider;
    private readonly LowLevelMouseProc _proc;
    private IntPtr _hook;
    private bool _dragging;
    private Point _lastPoint;
    private DateTime _lastDownUtc;
    private Point _lastDownPoint;

    public PipInteractionHook(WindowController window, Func<IntPtr> handleProvider)
    {
        _window = window;
        _handleProvider = handleProvider;
        _proc = HookCallback;
    }

    public static bool ShouldStartMove(Point cursor, Rectangle bounds, bool altHeld, int topGrabHeight = 12) =>
        bounds.Contains(cursor) && (altHeld || cursor.Y < bounds.Top + topGrabHeight);

    public static Rectangle NormalizeBounds(Rectangle requested, Rectangle workArea) =>
        PipGeometry.Normalize16By9(requested, workArea);

    public static bool NeedsNormalization(Rectangle bounds) =>
        Math.Abs(bounds.Height - (int)Math.Round(bounds.Width * 9d / 16d)) > 2;
    public void Start()
    {
        if (_hook != IntPtr.Zero) return;
        _hook = SetWindowsHookEx(WhMouseLl, _proc, IntPtr.Zero, 0);
        if (_hook == IntPtr.Zero)
            throw new InvalidOperationException("Could not install PiP mouse hook.");
    }

    private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code < 0 || !_window.IsPip)
            return CallNextHookEx(_hook, code, wParam, lParam);

        var data = Marshal.PtrToStructure<MsLlHookStruct>(lParam);
        var point = new Point(data.pt.x, data.pt.y);
        var message = wParam.ToInt32();

        if (message == WmLButtonDown && TryGetBounds(out var bounds))
        {
            if (IsDoubleClick(point, bounds))
            {
                _window.TogglePip();
                return (IntPtr)1;
            }

            var altHeld = (GetAsyncKeyState(VkMenu) & 0x8000) != 0;
            if (ShouldStartMove(point, bounds, altHeld))
            {
                _dragging = true;
                _lastPoint = point;
                return (IntPtr)1;
            }
        }

        if (message == WmMouseMove && _dragging)
        {
            _window.MoveBy(point.X - _lastPoint.X, point.Y - _lastPoint.Y);
            _lastPoint = point;
            return (IntPtr)1;
        }

        if (message == WmLButtonUp && _dragging)
        {
            _dragging = false;
            _window.CaptureCurrentPipBounds();
            return (IntPtr)1;
        }

        if (message == WmLButtonUp && TryGetBounds(out var resizedBounds) && NeedsNormalization(resizedBounds))
            _window.CaptureCurrentPipBounds();

        return CallNextHookEx(_hook, code, wParam, lParam);
    }

    private bool IsDoubleClick(Point point, Rectangle bounds)
    {
        if (!bounds.Contains(point)) return false;
        var now = DateTime.UtcNow;
        var withinTime = (now - _lastDownUtc).TotalMilliseconds <= SystemInformation.DoubleClickTime;
        var size = SystemInformation.DoubleClickSize;
        var withinArea = Math.Abs(point.X - _lastDownPoint.X) <= size.Width / 2 &&
                         Math.Abs(point.Y - _lastDownPoint.Y) <= size.Height / 2;
        _lastDownUtc = now;
        _lastDownPoint = point;
        return withinTime && withinArea;
    }
    private bool TryGetBounds(out Rectangle bounds)
    {
        bounds = Rectangle.Empty;
        var hwnd = _handleProvider();
        if (hwnd == IntPtr.Zero || !NativeMethods.GetWindowRect(hwnd, out var r)) return false;
        bounds = Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
        return true;
    }

    public void Dispose()
    {
        if (_hook == IntPtr.Zero) return;
        UnhookWindowsHookEx(_hook);
        _hook = IntPtr.Zero;
    }

    private delegate IntPtr LowLevelMouseProc(int code, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct PointNative { public int x; public int y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MsLlHookStruct
    {
        public PointNative pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc callback, IntPtr module, uint threadId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hook);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);
}

