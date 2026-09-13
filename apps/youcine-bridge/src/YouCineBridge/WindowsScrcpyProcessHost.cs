using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YouCineBridge;

public sealed class WindowsScrcpyProcessHost : IScrcpyProcessHost
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public ManagedProcess? FindManagedWindow(string title)
    {
        foreach (var process in Process.GetProcessesByName("scrcpy"))
        {
            try
            {
                if (process.MainWindowTitle == title && process.MainWindowHandle != IntPtr.Zero)
                    return new ManagedProcess(process.Id, process.MainWindowHandle);
            }
            finally
            {
                process.Dispose();
            }
        }
        return null;
    }

    public ManagedProcess Start(string fileName, IReadOnlyList<string> args)
    {
        var psi = new ProcessStartInfo(fileName) { UseShellExecute = false };
        foreach (var arg in args) psi.ArgumentList.Add(arg);
        var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start scrcpy.");
        var result = new ManagedProcess(process.Id, process.MainWindowHandle);
        process.Dispose();
        return result;
    }

    public void Focus(ManagedProcess process)
    {
        var handle = process.MainWindowHandle;
        if (handle == IntPtr.Zero)
        {
            using var p = Process.GetProcessById(process.Id);
            handle = p.MainWindowHandle;
        }
        if (handle != IntPtr.Zero) SetForegroundWindow(handle);
    }

    public void Stop(ManagedProcess process)
    {
        try
        {
            using var p = Process.GetProcessById(process.Id);
            if (!p.HasExited)
            {
                p.CloseMainWindow();
                if (!p.WaitForExit(1200)) p.Kill(entireProcessTree: true);
            }
        }
        catch (ArgumentException)
        {
            // Process already exited.
        }
    }
}
