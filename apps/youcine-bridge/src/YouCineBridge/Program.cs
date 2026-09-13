namespace YouCineBridge;

static class Program
{
    private const string InstanceName = "YouCineBridge";
    private const string ManagedWindowTitle = "YouCine-PC";
    private static readonly string[] FallbackEndpoints = ["100.106.31.127:38177"];

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var gate = new SingleInstanceGate(InstanceName);
        if (!gate.IsPrimary)
        {
            gate.SignalPrimary();
            return;
        }

        var appDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YouCineBridge");
        var settingsPath = Path.Combine(appDirectory, "settings.json");
        var logsDirectory = Path.Combine(appDirectory, "logs");
        Directory.CreateDirectory(appDirectory);
        var logger = new BridgeLogger(logsDirectory);

        BridgeApplicationContext context;
        try
        {
            var settings = BridgeSettings.Load(settingsPath);
            context = CreateContext(settings, settingsPath, logger);
        }
        catch (Exception ex)
        {
            logger.Log($"startup: {ex}");
            MessageBox.Show(
                ex.Message,
                "YouCine Bridge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        gate.ActivationRequested += (_, _) => context.RequestOpen();
        Application.Run(context);
    }

    private static BridgeApplicationContext CreateContext(
        BridgeSettings settings,
        string settingsPath,
        BridgeLogger logger)
    {
        var adbPath = ToolLocator.FindAdb(settings);
        var scrcpyPath = ToolLocator.FindScrcpy(settings);
        if (adbPath is null || scrcpyPath is null)
            return CreateUnavailableContext(adbPath, scrcpyPath, logger);

        var runner = new ProcessRunner();
        var resolver = new DeviceResolver(
            adbPath,
            runner,
            settings,
            FallbackEndpoints);
        var host = new WindowsScrcpyProcessHost();
        var session = new ScrcpySession(
            scrcpyPath,
            adbPath,
            runner,
            host,
            ScrcpyProfile.Balanced);

        IntPtr HandleProvider() =>
            host.FindManagedWindow(ManagedWindowTitle)?.MainWindowHandle ?? IntPtr.Zero;

        var window = new WindowController(
            HandleProvider,
            new WindowsWindowApi(),
            settings,
            settingsPath);
        var waiter = new WindowReadyWaiter(HandleProvider);
        var coordinator = new BridgeRuntimeCoordinator(
            settings,
            settingsPath,
            resolver.ResolveAsync,
            session.EnsureRunningAsync,
            () => session.Stop());

        async Task PrepareWindowAsync(CancellationToken ct)
        {
            await waiter.WaitAsync(TimeSpan.FromSeconds(15), ct);
            window.SynchronizePresentationState();
        }

        async Task OpenAsync(CancellationToken ct)
        {
            await coordinator.OpenAsync(ct);
            await PrepareWindowAsync(ct);
        }

        async Task ReconnectAsync(CancellationToken ct)
        {
            await coordinator.ReconnectAsync(ct);
            await PrepareWindowAsync(ct);
        }

        var router = new BridgeCommandRouter(
            window.TogglePip,
            session.Focus,
            ReconnectAsync,
            window.ToggleFullscreen);
        var lifecycle = new BridgeLifecycle(
            OpenAsync,
            router.ExecuteAsync,
            coordinator.Stop);
        var watchdog = new SessionWatchdog(
            () => coordinator.SessionExpected,
            () => HandleProvider() != IntPtr.Zero,
            OpenAsync,
            new RetryPolicy(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)));

        BridgeApplicationContext? context = null;
        var hotkeys = new HotkeyController(
            hotkey => context!.DispatchHotkey(hotkey),
            () => ForegroundWindowProbe.IsForeground(HandleProvider()));
        var pipHook = new PipInteractionHook(window, HandleProvider);
        context = new BridgeApplicationContext(
            lifecycle,
            logger,
            logger.DirectoryPath,
            hotkeys,
            pipHook,
            watchdog);
        return context;
    }

    private static BridgeApplicationContext CreateUnavailableContext(
        string? adbPath,
        string? scrcpyPath,
        BridgeLogger logger)
    {
        var missing = adbPath is null && scrcpyPath is null
            ? "adb.exe and scrcpy.exe were not found."
            : adbPath is null
                ? "adb.exe was not found."
                : "scrcpy.exe was not found.";
        var error = new FileNotFoundException(missing);
        Task Fail(CancellationToken _) => Task.FromException(error);
        Task FailHotkey(BridgeHotkey _, CancellationToken __) => Task.FromException(error);
        var lifecycle = new BridgeLifecycle(Fail, FailHotkey, () => { });
        return new BridgeApplicationContext(lifecycle, logger, logger.DirectoryPath);
    }
}
