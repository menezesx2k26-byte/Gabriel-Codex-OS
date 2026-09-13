using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class ScrcpySessionTests
{
    [Fact]
    public async Task EnsureRunningAsync_FocusesExistingManagedWindowWithoutStartingAnother()
    {
        var host = new FakeHost { Existing = new ManagedProcess(42, new IntPtr(1234)) };
        var runner = new RecordingRunner();
        var session = new ScrcpySession("scrcpy.exe", "adb.exe", runner, host, ScrcpyProfile.Balanced);

        await session.EnsureRunningAsync("100.106.31.127:38177", CancellationToken.None);

        Assert.Equal(1, host.FocusCount);
        Assert.Equal(0, host.StartCount);
    }

    [Fact]
    public async Task RestartAsync_ForceStopsOnlyYouCinePackageAndStartsFreshSession()
    {
        var host = new FakeHost { Existing = new ManagedProcess(42, new IntPtr(1234)) };
        var runner = new RecordingRunner();
        var session = new ScrcpySession("scrcpy.exe", "adb.exe", runner, host, ScrcpyProfile.Balanced);
        await session.EnsureRunningAsync("100.106.31.127:38177", CancellationToken.None);
        host.Existing = new ManagedProcess(42, new IntPtr(1234));

        await session.RestartAsync(CancellationToken.None);

        Assert.Contains(runner.Calls, c => c.SequenceEqual(new[] { "-s", "100.106.31.127:38177", "shell", "am", "force-stop", "com.world.youcinemobile" }));
        Assert.Equal(1, host.StopCount);
        Assert.Equal(1, host.StartCount);
    }

    private sealed class RecordingRunner : IProcessRunner
    {
        public List<IReadOnlyList<string>> Calls { get; } = new();
        public Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> args, CancellationToken ct)
        {
            Calls.Add(args.ToArray());
            return Task.FromResult(new ProcessResult(0, "", ""));
        }
    }

    private sealed class FakeHost : IScrcpyProcessHost
    {
        public ManagedProcess? Existing { get; set; }
        public int FocusCount { get; private set; }
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }
        public ManagedProcess? FindManagedWindow(string title) => Existing;
        public ManagedProcess Start(string fileName, IReadOnlyList<string> args) { StartCount++; Existing = new ManagedProcess(99, IntPtr.Zero); return Existing; }
        public void Focus(ManagedProcess process) => FocusCount++;
        public void Stop(ManagedProcess process) { StopCount++; Existing = null; }
    }
}
