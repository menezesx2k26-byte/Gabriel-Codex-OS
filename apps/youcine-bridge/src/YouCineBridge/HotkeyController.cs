using System.Runtime.InteropServices;

namespace YouCineBridge;

public enum BridgeHotkey
{
    TogglePip,
    Focus,
    Reconnect,
    ToggleFullscreen
}

public sealed class HotkeyController : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModControlAlt = 0x0003;
    private readonly Action<BridgeHotkey> _dispatch;
    private readonly Func<bool> _isYouCineFocused;
    private readonly HotkeyWindow _window;
    private readonly System.Windows.Forms.Timer _focusTimer;
    private bool _f11Registered;
    private bool _started;
    public HotkeyController(Action<BridgeHotkey> dispatch, Func<bool> isYouCineFocused)
    {
        _dispatch = dispatch;
        _isYouCineFocused = isYouCineFocused;
        _window = new HotkeyWindow(HandleHotkey);
        _focusTimer = new System.Windows.Forms.Timer { Interval = 200 };
        _focusTimer.Tick += (_, _) => SyncF11Registration();
    }

    public static BridgeHotkey? ResolveAction(int id, bool youCineFocused) => id switch
    {
        1 => BridgeHotkey.TogglePip,
        2 => BridgeHotkey.Focus,
        3 => BridgeHotkey.Reconnect,
        4 when youCineFocused => BridgeHotkey.ToggleFullscreen,
        _ => null
    };

    public void Start()
    {
        if (_started) return;
        RegisterRequired(1, ModControlAlt, (uint)Keys.P);
        RegisterRequired(2, ModControlAlt, (uint)Keys.Y);
        RegisterRequired(3, ModControlAlt, (uint)Keys.R);
        _focusTimer.Start();
        SyncF11Registration();
        _started = true;
    }
    private void HandleHotkey(int id)
    {
        var action = ResolveAction(id, _isYouCineFocused());
        if (action is BridgeHotkey value) _dispatch(value);
    }

    private void SyncF11Registration()
    {
        var shouldRegister = _isYouCineFocused();
        if (shouldRegister == _f11Registered) return;
        if (shouldRegister)
        {
            RegisterRequired(4, 0, (uint)Keys.F11);
            _f11Registered = true;
        }
        else
        {
            UnregisterHotKey(_window.Handle, 4);
            _f11Registered = false;
        }
    }

    private void RegisterRequired(int id, uint modifiers, uint key)
    {
        if (!RegisterHotKey(_window.Handle, id, modifiers, key))
            throw new InvalidOperationException($"Nao foi possivel registrar hotkey {id}.");
    }

    public void Dispose()
    {
        _focusTimer.Stop();
        for (var id = 1; id <= 4; id++) UnregisterHotKey(_window.Handle, id);
        _window.Dispose();
        _focusTimer.Dispose();
    }
    private sealed class HotkeyWindow : NativeWindow, IDisposable
    {
        private readonly Action<int> _handler;

        public HotkeyWindow(Action<int> handler)
        {
            _handler = handler;
            CreateHandle(new CreateParams
            {
                Caption = "YouCineBridge.Hotkeys",
                Parent = new IntPtr(-3)
            });
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey) _handler(m.WParam.ToInt32());
            base.WndProc(ref m);
        }

        public void Dispose() => DestroyHandle();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
