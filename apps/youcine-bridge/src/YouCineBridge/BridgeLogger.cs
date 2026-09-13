using System.Text;

namespace YouCineBridge;

public sealed class BridgeLogger
{
    private readonly string _dir;
    private readonly long _maxBytes;
    private readonly int _backups;
    private readonly object _sync = new();

    public BridgeLogger(string dir, long maxBytes = 1_048_576, int backups = 3)
    {
        _dir = dir;
        _maxBytes = Math.Max(1, maxBytes);
        _backups = Math.Max(1, backups);
    }

    public string DirectoryPath => _dir;

    public void Log(string message)
    {
        lock (_sync)
        {
            Directory.CreateDirectory(_dir);
            var current = Path.Combine(_dir, "bridge.log");
            if (File.Exists(current) && new FileInfo(current).Length >= _maxBytes)
                Rotate(current);

            var line = $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}";
            File.AppendAllText(current, line, Encoding.UTF8);
        }
    }

    private void Rotate(string current)
    {
        for (var index = _backups; index >= 1; index--)
        {
            var target = Path.Combine(_dir, $"bridge.{index}.log");
            var source = index == 1
                ? current
                : Path.Combine(_dir, $"bridge.{index - 1}.log");

            if (File.Exists(target)) File.Delete(target);
            if (File.Exists(source)) File.Move(source, target);
        }
    }
}
