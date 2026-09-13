namespace YouCineBridge;

public static class ToolLocator
{
    public static string? FindAdb(
        BridgeSettings settings,
        Func<string, bool>? fileExists = null,
        string? localAppData = null)
    {
        fileExists ??= File.Exists;
        localAppData ??= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(settings.AdbPathOverride) && fileExists(settings.AdbPathOverride))
            return settings.AdbPathOverride;

        var known = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages",
            "Google.PlatformTools_Microsoft.Winget.Source_8wekyb3d8bbwe", "platform-tools", "adb.exe");
        return fileExists(known) ? known : null;
    }

    public static string? FindScrcpy(
        BridgeSettings settings,
        Func<string, bool>? fileExists = null,
        string? localAppData = null,
        Func<string, IEnumerable<string>>? enumerateFiles = null)
    {
        fileExists ??= File.Exists;
        localAppData ??= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        enumerateFiles ??= pattern => Directory.EnumerateFiles(
            Path.GetDirectoryName(pattern)!, Path.GetFileName(pattern), SearchOption.AllDirectories);

        if (!string.IsNullOrWhiteSpace(settings.ScrcpyPathOverride) && fileExists(settings.ScrcpyPathOverride))
            return settings.ScrcpyPathOverride;

        var packageRoot = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages",
            "Genymobile.scrcpy_Microsoft.Winget.Source_8wekyb3d8bbwe");
        if (!Directory.Exists(packageRoot)) return null;
        return enumerateFiles(Path.Combine(packageRoot, "scrcpy.exe")).FirstOrDefault(fileExists);
    }
}
