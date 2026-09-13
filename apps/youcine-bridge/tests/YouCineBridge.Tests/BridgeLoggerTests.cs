using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class BridgeLoggerTests
{
    [Fact]
    public void Log_CreatesCurrentLog()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"youcine-logs-{Guid.NewGuid():N}");
        try
        {
            var logger = new BridgeLogger(dir, maxBytes: 1024, backups: 2);
            logger.Log("hello");
            var path = Path.Combine(dir, "bridge.log");
            Assert.True(File.Exists(path));
            Assert.Contains("hello", File.ReadAllText(path));
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public void Log_RotatesOversizedCurrentFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"youcine-logs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "bridge.log"), new string('x', 50));
            var logger = new BridgeLogger(dir, maxBytes: 20, backups: 2);
            logger.Log("new");
            Assert.True(File.Exists(Path.Combine(dir, "bridge.1.log")));
            Assert.Contains("new", File.ReadAllText(Path.Combine(dir, "bridge.log")));
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }
}
