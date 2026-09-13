using System.Drawing;
using System.Text.Json;

namespace YouCineBridge;

public sealed class BridgeSettings
{
    public string? RuntimeEndpoint { get; set; }
    public string? LastEndpoint { get; set; }
    public Rectangle? PipBounds { get; set; }
    public string? AdbPathOverride { get; set; }
    public string? ScrcpyPathOverride { get; set; }

    public static BridgeSettings Load(string path)
    {
        if (!File.Exists(path)) return new BridgeSettings();
        var dto = JsonSerializer.Deserialize<SettingsDto>(File.ReadAllText(path));
        if (dto is null) return new BridgeSettings();
        return new BridgeSettings
        {
            RuntimeEndpoint = dto.RuntimeEndpoint,
            LastEndpoint = dto.LastEndpoint,
            PipBounds = dto.PipBounds is null ? null : new Rectangle(
                dto.PipBounds.X, dto.PipBounds.Y, dto.PipBounds.Width, dto.PipBounds.Height),
            AdbPathOverride = dto.AdbPathOverride,
            ScrcpyPathOverride = dto.ScrcpyPathOverride
        };
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        var dto = new SettingsDto(
            RuntimeEndpoint,
            LastEndpoint,
            PipBounds is Rectangle r ? new RectDto(r.X, r.Y, r.Width, r.Height) : null,
            AdbPathOverride,
            ScrcpyPathOverride);
        File.WriteAllText(path, JsonSerializer.Serialize(dto,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed record SettingsDto(
        string? RuntimeEndpoint,
        string? LastEndpoint,
        RectDto? PipBounds,
        string? AdbPathOverride,
        string? ScrcpyPathOverride);

    private sealed record RectDto(int X, int Y, int Width, int Height);
}
