using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YouCineBridge;

public sealed record ManagedProcess(int Id, IntPtr MainWindowHandle);

public interface IScrcpyProcessHost
{
    ManagedProcess? FindManagedWindow(string title);
    ManagedProcess Start(string fileName, IReadOnlyList<string> args);
    void Focus(ManagedProcess process);
    void Stop(ManagedProcess process);
}

public sealed class ScrcpySession
{
    private const string WindowTitle = "YouCine-PC";
    private const string PackageName = "com.world.youcinemobile";
    private readonly string _scrcpyPath;
    private readonly string _adbPath;
    private readonly IProcessRunner _runner;
    private readonly IScrcpyProcessHost _host;
    private readonly ScrcpyProfile _profile;
    private string? _endpoint;

    public ScrcpySession(string scrcpyPath, string adbPath, IProcessRunner runner, IScrcpyProcessHost host, ScrcpyProfile profile)
    {
        _scrcpyPath = scrcpyPath;
        _adbPath = adbPath;
        _runner = runner;
        _host = host;
        _profile = profile;
    }

    public Task EnsureRunningAsync(string endpoint, CancellationToken ct)
    {
        _endpoint = endpoint;
        var existing = _host.FindManagedWindow(WindowTitle);
        if (existing is not null)
        {
            _host.Focus(existing);
            return Task.CompletedTask;
        }

        _host.Start(_scrcpyPath, _profile.BuildArguments(endpoint));
        return Task.CompletedTask;
    }

    public async Task RestartAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
            throw new InvalidOperationException("No endpoint has been resolved yet.");

        var existing = _host.FindManagedWindow(WindowTitle);
        if (existing is not null) _host.Stop(existing);
        await _runner.RunAsync(_adbPath,
            new[] { "-s", _endpoint, "shell", "am", "force-stop", PackageName }, ct);
        _host.Start(_scrcpyPath, _profile.BuildArguments(_endpoint));
    }

    public void Focus()
    {
        var existing = _host.FindManagedWindow(WindowTitle);
        if (existing is not null) _host.Focus(existing);
    }

    public void Stop(bool userRequested = true)
    {
        var existing = _host.FindManagedWindow(WindowTitle);
        if (existing is not null) _host.Stop(existing);
    }
}
