using System.Drawing;
using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class BridgeSettingsTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsEndpointAndPipBounds()
    {
        var path = Path.Combine(Path.GetTempPath(), $"youcine-{Guid.NewGuid():N}.json");
        try
        {
            var original = new BridgeSettings
            {
                RuntimeEndpoint = "100.80.1.9:5555",
                LastEndpoint = "100.106.31.127:38177",
                PipBounds = new Rectangle(123, 234, 640, 360),
                AdbPathOverride = @"C:\tools\adb.exe",
                ScrcpyPathOverride = @"C:\tools\scrcpy.exe"
            };

            original.Save(path);
            var loaded = BridgeSettings.Load(path);
            Assert.Equal(original.RuntimeEndpoint, loaded.RuntimeEndpoint);
            Assert.Equal(original.LastEndpoint, loaded.LastEndpoint);
            Assert.Equal(original.PipBounds, loaded.PipBounds);
            Assert.Equal(original.AdbPathOverride, loaded.AdbPathOverride);
            Assert.Equal(original.ScrcpyPathOverride, loaded.ScrcpyPathOverride);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
