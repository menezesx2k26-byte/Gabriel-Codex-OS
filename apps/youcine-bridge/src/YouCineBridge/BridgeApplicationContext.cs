using System.Diagnostics;
using System.Drawing;

namespace YouCineBridge;

public enum BridgeTrayCommand
{
    Open,
    TogglePip,
    Reconnect,
    OpenLogs,
    Exit
}

public sealed record BridgeTrayMenuItem(string Text, BridgeTrayCommand Command);

public sealed class BridgeApplicationContext : ApplicationContext
{
    private readonly BridgeLifecycle _lifecycle;
    private readonly HotkeyController? _hotkeys;
    private readonly PipInteractionHook? _pipHook;
    private readonly SessionWatchdog? _watchdog;
    private readonly BridgeLogger _logger;
    private readonly string _logsDirectory;
    private readonly NotifyIcon _trayIcon;
    private readonly Control _dispatcher = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _watchdogTask;
    private bool _started;
    private bool _shuttingDown;

    public static IReadOnlyList<BridgeTrayMenuItem> TrayMenu { get; } =
    [
        new("Open YouCine", BridgeTrayCommand.Open),
        new("PiP / Fullscreen", BridgeTrayCommand.TogglePip),
        new("Reconnect", BridgeTrayCommand.Reconnect),
        new("Open logs", BridgeTrayCommand.OpenLogs),
        new("Exit", BridgeTrayCommand.Exit)
    ];

    public BridgeApplicationContext(
        BridgeLifecycle lifecycle,
        BridgeLogger logger,
        string logsDirectory,
        HotkeyController? hotkeys = null,
        PipInteractionHook? pipHook = null,
        SessionWatchdog? watchdog = null)
    {
        _lifecycle = lifecycle;
        _logger = logger;
        _logsDirectory = logsDirectory;
        _hotkeys = hotkeys;
        _pipHook = pipHook;
        _watchdog = watchdog;
        Directory.CreateDirectory(logsDirectory);
        _trayIcon = BuildTrayIcon();
        _dispatcher.CreateControl();
        Application.Idle += OnFirstIdle;
    }

    private NotifyIcon BuildTrayIcon()
    {
        var menu = new ContextMenuStrip();
        foreach (var entry in TrayMenu)
        {
            var item = new ToolStripMenuItem(entry.Text) { Tag = entry.Command };
            item.Click += async (_, _) => await RunTrayCommandAsync(entry.Command);
            menu.Items.Add(item);
        }

        var tray = new NotifyIcon
        {
            Text = "YouCine Bridge",
            Icon = SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };
        tray.DoubleClick += async (_, _) => await RunTrayCommandAsync(BridgeTrayCommand.Open);
        return tray;
    }

    private async void OnFirstIdle(object? sender, EventArgs e)
    {
        if (_started) return;
        _started = true;
        Application.Idle -= OnFirstIdle;
        try
        {
            _hotkeys?.Start();
            _pipHook?.Start();
            await RunTrayCommandAsync(BridgeTrayCommand.Open);
            if (_watchdog is not null)
                _watchdogTask = _watchdog.RunAsync(TimeSpan.FromSeconds(3), _cts.Token);
        }
        catch (Exception ex)
        {
            ReportFailure("startup", ex);
        }
    }

    public void DispatchHotkey(BridgeHotkey hotkey)
    {
        _ = RunHotkeyAsync(hotkey);
    }

    public void RequestOpen()
    {
        if (_shuttingDown || _dispatcher.IsDisposed) return;
        if (_dispatcher.InvokeRequired)
        {
            _dispatcher.BeginInvoke((Action)RequestOpen);
            return;
        }
        _ = RunTrayCommandAsync(BridgeTrayCommand.Open);
    }

    private async Task RunHotkeyAsync(BridgeHotkey hotkey)
    {
        try
        {
            await _lifecycle.ExecuteAsync(hotkey, _cts.Token);
        }
        catch (Exception ex)
        {
            ReportFailure(hotkey.ToString(), ex);
        }
    }

    private async Task RunTrayCommandAsync(BridgeTrayCommand command)
    {
        try
        {
            await ExecuteTrayCommandAsync(command, _lifecycle, OpenLogs, Shutdown, _cts.Token);
        }
        catch (OperationCanceledException) when (_cts.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            ReportFailure(command.ToString(), ex);
        }
    }

    private void OpenLogs()
    {
        Directory.CreateDirectory(_logsDirectory);
        Process.Start(new ProcessStartInfo(_logsDirectory) { UseShellExecute = true });
    }

    private void ReportFailure(string operation, Exception ex)
    {
        _logger.Log($"{operation}: {ex}");
        _trayIcon.ShowBalloonTip(
            5000,
            "YouCine Bridge",
            ex.Message,
            ToolTipIcon.Error);
    }

    private void Shutdown()
    {
        if (_shuttingDown) return;
        _shuttingDown = true;
        _cts.Cancel();
        _hotkeys?.Dispose();
        _pipHook?.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _dispatcher.Dispose();
        ExitThread();
    }

    public static async Task ExecuteTrayCommandAsync(
        BridgeTrayCommand command,
        BridgeLifecycle lifecycle,
        Action openLogs,
        Action exit,
        CancellationToken ct)
    {
        switch (command)
        {
            case BridgeTrayCommand.Open:
                await lifecycle.StartAsync(ct);
                break;
            case BridgeTrayCommand.TogglePip:
                await lifecycle.ExecuteAsync(BridgeHotkey.TogglePip, ct);
                break;
            case BridgeTrayCommand.Reconnect:
                await lifecycle.ExecuteAsync(BridgeHotkey.Reconnect, ct);
                break;
            case BridgeTrayCommand.OpenLogs:
                openLogs();
                break;
            case BridgeTrayCommand.Exit:
                lifecycle.Stop();
                exit();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
    }
}
