using System.Drawing;
using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class WindowControllerTests
{
    [Fact]
    public void EnterPip_RemovesCaption_KeepsResize_AndSetsTopmost()
    {
        var api = new FakeWindowApi();
        var settings = new BridgeSettings();
        var controller = new WindowController(() => (IntPtr)42, api, settings, "settings.json");
        var requested = new Rectangle(100, 80, 640, 360);

        controller.EnterPip(requested);

        Assert.Equal(0, api.Style & NativeMethods.WS_CAPTION);
        Assert.NotEqual(0, api.Style & NativeMethods.WS_THICKFRAME);
        Assert.NotEqual(0, api.ExStyle & NativeMethods.WS_EX_TOPMOST);
        Assert.Equal(requested, api.Bounds);
        Assert.True(api.LastTopmost);
    }

    [Fact]
    public void EnterFullscreen_RemovesFrame_FillsMonitor_AndClearsTopmost()
    {
        var api = new FakeWindowApi
        {
            MonitorBounds = new Rectangle(0, 0, 1920, 1080),
            ExStyle = NativeMethods.WS_EX_TOPMOST
        };
        var controller = new WindowController(() => (IntPtr)42, api, new BridgeSettings(), "settings.json");

        controller.EnterFullscreen();

        Assert.Equal(0, api.Style & (NativeMethods.WS_CAPTION | NativeMethods.WS_THICKFRAME));
        Assert.Equal(0, api.ExStyle & NativeMethods.WS_EX_TOPMOST);
        Assert.Equal(api.MonitorBounds, api.Bounds);
        Assert.False(api.LastTopmost);
    }

    private sealed class FakeWindowApi : IWindowApi
    {
        public long Style { get; set; } = NativeMethods.WS_VISIBLE | NativeMethods.WS_CAPTION | NativeMethods.WS_THICKFRAME;
        public long ExStyle { get; set; }
        public Rectangle Bounds { get; set; } = new(50, 50, 1280, 720);
        public Rectangle MonitorBounds { get; set; } = new(0, 0, 1280, 720);
        public bool LastTopmost { get; private set; }
        public long GetStyle(IntPtr hwnd) => Style;
        public long GetExStyle(IntPtr hwnd) => ExStyle;
        public Rectangle GetBounds(IntPtr hwnd) => Bounds;
        public Rectangle GetMonitorBounds(IntPtr hwnd) => MonitorBounds;
        public void SetStyle(IntPtr hwnd, long style) => Style = style;
        public void SetExStyle(IntPtr hwnd, long style) => ExStyle = style;
        public void SetBounds(IntPtr hwnd, Rectangle bounds, bool topmost, bool frameChanged)
        {
            Bounds = bounds;
            LastTopmost = topmost;
        }
        public void Focus(IntPtr hwnd) { }
    }
}

public sealed class WindowedToggleTests
{
    [Fact]
    public void ToggleFullscreen_FromFullscreen_RestoresFramedWindow()
    {
        var api = new ToggleFakeWindowApi();
        var controller = new WindowController(() => (IntPtr)42, api, new BridgeSettings(), "settings.json");
        controller.EnterFullscreen();

        controller.ToggleFullscreen();

        Assert.NotEqual(0, api.Style & NativeMethods.WS_CAPTION);
        Assert.NotEqual(0, api.Style & NativeMethods.WS_THICKFRAME);
        Assert.False(controller.IsFullscreen);
    }

    private sealed class ToggleFakeWindowApi : IWindowApi
    {
        public long Style { get; set; } = NativeMethods.WS_VISIBLE | NativeMethods.WS_CAPTION | NativeMethods.WS_THICKFRAME;
        public long ExStyle { get; set; }
        public Rectangle Bounds { get; set; } = new(0, 0, 1280, 720);
        public long GetStyle(IntPtr hwnd) => Style;
        public long GetExStyle(IntPtr hwnd) => ExStyle;
        public Rectangle GetBounds(IntPtr hwnd) => Bounds;
        public Rectangle GetMonitorBounds(IntPtr hwnd) => new(0, 0, 1920, 1080);
        public void SetStyle(IntPtr hwnd, long style) => Style = style;
        public void SetExStyle(IntPtr hwnd, long style) => ExStyle = style;
        public void SetBounds(IntPtr hwnd, Rectangle bounds, bool topmost, bool frameChanged) => Bounds = bounds;
        public void Focus(IntPtr hwnd) { }
    }
}

public sealed class WindowStateSyncTests
{
    [Fact]
    public void SynchronizePresentationState_DetectsExistingFullscreenWindow()
    {
        var api = new StateFakeWindowApi
        {
            Style = NativeMethods.WS_VISIBLE,
            ExStyle = 0,
            Bounds = new Rectangle(0, 0, 1920, 1080)
        };
        var controller = new WindowController(() => (IntPtr)42, api, new BridgeSettings(), "settings.json");

        controller.SynchronizePresentationState();

        Assert.True(controller.IsFullscreen);
        Assert.False(controller.IsPip);
    }

    [Fact]
    public void SynchronizePresentationState_DetectsExistingPipWindow()
    {
        var api = new StateFakeWindowApi
        {
            Style = NativeMethods.WS_VISIBLE | NativeMethods.WS_THICKFRAME,
            ExStyle = NativeMethods.WS_EX_TOPMOST,
            Bounds = new Rectangle(100, 100, 640, 360)
        };
        var controller = new WindowController(() => (IntPtr)42, api, new BridgeSettings(), "settings.json");

        controller.SynchronizePresentationState();

        Assert.True(controller.IsPip);
        Assert.False(controller.IsFullscreen);
    }

    private sealed class StateFakeWindowApi : IWindowApi
    {
        public long Style { get; set; }
        public long ExStyle { get; set; }
        public Rectangle Bounds { get; set; }
        public Rectangle MonitorBounds { get; set; } = new(0, 0, 1920, 1080);
        public long GetStyle(IntPtr hwnd) => Style;
        public long GetExStyle(IntPtr hwnd) => ExStyle;
        public Rectangle GetBounds(IntPtr hwnd) => Bounds;
        public Rectangle GetMonitorBounds(IntPtr hwnd) => MonitorBounds;
        public void SetStyle(IntPtr hwnd, long style) => Style = style;
        public void SetExStyle(IntPtr hwnd, long style) => ExStyle = style;
        public void SetBounds(IntPtr hwnd, Rectangle bounds, bool topmost, bool frameChanged) => Bounds = bounds;
        public void Focus(IntPtr hwnd) { }
    }
}
