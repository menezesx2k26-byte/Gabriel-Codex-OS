using System.Diagnostics;

namespace YouCineBridge;

public readonly record struct ProcessResult(int ExitCode, string StdOut, string StdErr);

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> args, CancellationToken ct);
}

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var arg in args) psi.ArgumentList.Add(arg);
        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Could not start {fileName}");
        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        return new ProcessResult(process.ExitCode, await stdoutTask, await stderrTask);
    }
}
