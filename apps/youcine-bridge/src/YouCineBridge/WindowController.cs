using System.Drawing;

namespace YouCineBridge;

public sealed class WindowController
{
    private const long FrameMask = NativeMethods.WS_CAPTION | NativeMethods.WS_THICKFRAME |
        NativeMethods.WS_SYSMENU | NativeMethods.WS_MINIMIZEBOX | NativeMethods.WS_MAXIMIZEBOX;
    private readonly Func<IntPtr> _handleProvider;
    private readonly IWindowApi _api;
    private readonly BridgeSettings _settings;
    private readonly string _settingsPath;

    public WindowController(Func<IntPtr> handleProvider, IWindowApi api,
        BridgeSettings settings, string settingsPath)
    {
        _handleProvider = handleProvider;
        _api = api;
        _settings = settings;
        _settingsPath = settingsPath;
    }

    public bool IsPip { get; private set; }

    public void EnterPip(Rectangle requested)
    {
        var hwnd = RequireHandle();
        var monitor = _api.GetMonitorBounds(hwnd);
        var bounds = PipGeometry.Normalize16By9(requested, monitor);
        var style = _api.GetStyle(hwnd);
        style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_SYSMENU |
            NativeMethods.WS_MINIMIZEBOX | NativeMethods.WS_MAXIMIZEBOX);
        style |= NativeMethods.WS_THICKFRAME;
        var exStyle = _api.GetExStyle(hwnd) | NativeMethods.WS_EX_TOPMOST;
        _api.SetStyle(hwnd, style);
        _api.SetExStyle(hwnd, exStyle);
        _api.SetBounds(hwnd, bounds, topmost: true, frameChanged: true);
        IsPip = true;
    }

    public void EnterFullscreen()
    {
        var hwnd = RequireHandle();
        var style = _api.GetStyle(hwnd) & ~FrameMask;
        var exStyle = _api.GetExStyle(hwnd) & ~NativeMethods.WS_EX_TOPMOST;
        _api.SetStyle(hwnd, style);
        _api.SetExStyle(hwnd, exStyle);
        _api.SetBounds(hwnd, _api.GetMonitorBounds(hwnd), topmost: false, frameChanged: true);
        IsPip = false;
    }

    public void TogglePip()
    {
        var hwnd = RequireHandle();
        if (IsPip)
        {
            CaptureCurrentPipBounds();
            EnterFullscreen();
            return;
        }

        var monitor = _api.GetMonitorBounds(hwnd);
        EnterPip(_settings.PipBounds ?? PipGeometry.DefaultFor(monitor));
    }
    public void MoveBy(int dx, int dy)
    {
        if (!IsPip) return;
        var hwnd = RequireHandle();
        var bounds = _api.GetBounds(hwnd);
        bounds.Offset(dx, dy);
        _api.SetBounds(hwnd, bounds, topmost: true, frameChanged: false);
    }

    public Rectangle CaptureCurrentPipBounds()
    {
        var hwnd = RequireHandle();
        var normalized = PipGeometry.Normalize16By9(
            _api.GetBounds(hwnd), _api.GetMonitorBounds(hwnd));
        _api.SetBounds(hwnd, normalized, topmost: true, frameChanged: false);
        _settings.PipBounds = normalized;
        _settings.Save(_settingsPath);
        return normalized;
    }

    public void Focus() => _api.Focus(RequireHandle());

    private IntPtr RequireHandle()
    {
        var hwnd = _handleProvider();
        if (hwnd == IntPtr.Zero)
            throw new InvalidOperationException("YouCine window is not ready yet.");
        return hwnd;
    }
}
