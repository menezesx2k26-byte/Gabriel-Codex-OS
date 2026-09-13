using System.Text.RegularExpressions;

namespace YouCineBridge;

public sealed record ResolvedDevice(string Endpoint, string? Model);

public sealed class DeviceResolver
{
    private static readonly Regex EndpointRegex = new(
        @"(?<endpoint>(?:\d{1,3}\.){3}\d{1,3}:\d+)", RegexOptions.Compiled);
    private readonly string _adbPath;
    private readonly IProcessRunner _runner;
    private readonly BridgeSettings _settings;
    private readonly IReadOnlyList<string> _fallbackEndpoints;
    private readonly string _expectedModel;

    public DeviceResolver(
        string adbPath,
        IProcessRunner runner,
        BridgeSettings settings,
        IReadOnlyList<string> fallbackEndpoints,
        string expectedModel = "moto_g75_5G")
    {
        _adbPath = adbPath;
        _runner = runner;
        _settings = settings;
        _fallbackEndpoints = fallbackEndpoints;
        _expectedModel = expectedModel;
    }
    public static IReadOnlyList<ResolvedDevice> ParseConnectedDevices(string output)
    {
        var result = new List<ResolvedDevice>();
        foreach (var raw in output.Split(new[] { '\r', '\n' },
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 ||
                !string.Equals(parts[1], "device", StringComparison.OrdinalIgnoreCase))
                continue;

            string? model = parts.FirstOrDefault(p =>
                p.StartsWith("model:", StringComparison.OrdinalIgnoreCase))?[6..];
            result.Add(new ResolvedDevice(parts[0], model));
        }
        return result;
    }

    public static IReadOnlyList<string> ParseMdnsEndpoints(string output) =>
        EndpointRegex.Matches(output)
            .Select(m => m.Groups["endpoint"].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public async Task<ResolvedDevice> ResolveAsync(CancellationToken ct)
    {
        var connected = await GetConnectedAsync(ct);
        if (!string.IsNullOrWhiteSpace(_settings.RuntimeEndpoint))
        {
            var runtime = connected.FirstOrDefault(d =>
                string.Equals(d.Endpoint, _settings.RuntimeEndpoint,
                    StringComparison.OrdinalIgnoreCase));
            if (runtime is not null) return Remember(runtime);

            runtime = await TryTrustedEndpointAsync(_settings.RuntimeEndpoint, ct);
            if (runtime is not null) return Remember(runtime);
        }

        var existingPhone = connected.FirstOrDefault(IsExpectedModel);
        if (existingPhone is not null) return Remember(existingPhone);

        if (!string.IsNullOrWhiteSpace(_settings.LastEndpoint) &&
            !string.Equals(_settings.LastEndpoint, _settings.RuntimeEndpoint,
                StringComparison.OrdinalIgnoreCase))
        {
            var remembered = await TryExpectedEndpointAsync(_settings.LastEndpoint, ct);
            if (remembered is not null) return Remember(remembered);
        }

        var mdns = await _runner.RunAsync(_adbPath,
            new[] { "mdns", "services" }, ct);
        var discovered = mdns.ExitCode == 0
            ? ParseMdnsEndpoints(mdns.StdOut)
            : Array.Empty<string>();
        foreach (var endpoint in discovered.Concat(_fallbackEndpoints)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.Equals(endpoint, _settings.RuntimeEndpoint,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(endpoint, _settings.LastEndpoint,
                    StringComparison.OrdinalIgnoreCase))
                continue;

            var match = await TryExpectedEndpointAsync(endpoint, ct);
            if (match is not null) return Remember(match);
        }

        throw new InvalidOperationException("Nenhum runtime Android confiavel encontrado via ADB.");
    }

    private async Task<ResolvedDevice?> TryTrustedEndpointAsync(
        string endpoint, CancellationToken ct)
    {
        await _runner.RunAsync(_adbPath, new[] { "connect", endpoint }, ct);
        var connected = await GetConnectedAsync(ct);
        return connected.FirstOrDefault(d => string.Equals(
            d.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase));
    }
    private async Task<ResolvedDevice?> TryExpectedEndpointAsync(
        string endpoint, CancellationToken ct)
    {
        await _runner.RunAsync(_adbPath, new[] { "connect", endpoint }, ct);
        var connected = await GetConnectedAsync(ct);
        return connected.FirstOrDefault(d =>
            string.Equals(d.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase) &&
            IsExpectedModel(d));
    }

    private async Task<IReadOnlyList<ResolvedDevice>> GetConnectedAsync(CancellationToken ct)
    {
        var result = await _runner.RunAsync(_adbPath,
            new[] { "devices", "-l" }, ct);
        return result.ExitCode == 0
            ? ParseConnectedDevices(result.StdOut)
            : Array.Empty<ResolvedDevice>();
    }

    private bool IsExpectedModel(ResolvedDevice device) =>
        string.Equals(device.Model, _expectedModel, StringComparison.OrdinalIgnoreCase);

    private ResolvedDevice Remember(ResolvedDevice device)
    {
        _settings.LastEndpoint = device.Endpoint;
        return device;
    }
}
