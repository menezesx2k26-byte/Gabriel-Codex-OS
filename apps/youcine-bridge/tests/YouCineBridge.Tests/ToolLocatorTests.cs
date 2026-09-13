using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class ToolLocatorTests
{
    [Fact]
    public void FindAdb_PrefersExplicitOverride()
    {
        var settings = new BridgeSettings { AdbPathOverride = @"C:\custom\adb.exe" };
        var found = ToolLocator.FindAdb(settings, _ => true, @"C:\LocalAppData");
        Assert.Equal(@"C:\custom\adb.exe", found);
    }

    [Fact]
    public void FindAdb_UsesKnownWingetPlatformToolsPath()
    {
        var root = @"C:\LocalAppData";
        var expected = Path.Combine(root, "Microsoft", "WinGet", "Packages",
            "Google.PlatformTools_Microsoft.Winget.Source_8wekyb3d8bbwe", "platform-tools", "adb.exe");
        var found = ToolLocator.FindAdb(new BridgeSettings(), p => p == expected, root);
        Assert.Equal(expected, found);
    }
}

public sealed class ScrcpyToolLocatorTests
{
    [Fact]
    public void FindScrcpy_PrefersExplicitOverride()
    {
        var settings = new BridgeSettings { ScrcpyPathOverride = @"C:\custom\scrcpy.exe" };
        var found = ToolLocator.FindScrcpy(settings, _ => true, @"C:\LocalAppData", _ => Array.Empty<string>());
        Assert.Equal(@"C:\custom\scrcpy.exe", found);
    }
}
