using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class DeviceResolverTests
{
    [Fact]
    public void ParseConnectedDevices_AcceptsOnlyDeviceTransports()
    {
        var output = "List of devices attached\r\n100.1.2.3:5555 device product:paros_g model:moto_g75_5G device:sorap\r\n192.168.1.9:5555 unauthorized\r\n";
        var devices = DeviceResolver.ParseConnectedDevices(output);
        var device = Assert.Single(devices);
        Assert.Equal("100.1.2.3:5555", device.Endpoint);
        Assert.Equal("moto_g75_5G", device.Model);
    }

    [Fact]
    public void ParseMdnsEndpoints_ExtractsConnectEndpointsWithoutDuplicates()
    {
        var output = "adb-PAROS._adb-tls-connect._tcp 192.168.15.4:38177\r\nadb-PAROS._adb-tls-connect._tcp 192.168.15.4:38177\r\n";
        var endpoints = DeviceResolver.ParseMdnsEndpoints(output);
        Assert.Equal(new[] { "192.168.15.4:38177" }, endpoints);
    }
    [Fact]
    public async Task ResolveAsync_TriesRuntimeEndpointBeforePhoneState()
    {
        var runner = new ScriptedRunner();
        runner.Enqueue("devices", new ProcessResult(0, "List of devices attached\r\n", ""));
        runner.Enqueue("connect", new ProcessResult(0, "connected to 100.80.1.9:5555", ""));
        runner.Enqueue("devices", new ProcessResult(0, "List of devices attached\r\n100.80.1.9:5555 device model:redroid\r\n", ""));
        var settings = new BridgeSettings
        {
            RuntimeEndpoint = "100.80.1.9:5555",
            LastEndpoint = "10.0.0.2:40000"
        };
        var resolver = new DeviceResolver("adb.exe", runner, settings, new[] { "100.106.31.127:38177" });

        var result = await resolver.ResolveAsync(CancellationToken.None);

        Assert.Equal("100.80.1.9:5555", result.Endpoint);
        Assert.Equal("redroid", result.Model);
        Assert.DoesNotContain(runner.Calls, c => c.Contains("10.0.0.2:40000"));
    }
    [Fact]
    public async Task ResolveAsync_TriesPersistedEndpointBeforeFallback()
    {
        var runner = new ScriptedRunner();
        runner.Enqueue("devices", new ProcessResult(0, "List of devices attached\r\n", ""));
        runner.Enqueue("connect", new ProcessResult(0, "connected to 10.0.0.2:40000", ""));
        runner.Enqueue("devices", new ProcessResult(0, "List of devices attached\r\n10.0.0.2:40000 device model:moto_g75_5G\r\n", ""));
        var settings = new BridgeSettings { LastEndpoint = "10.0.0.2:40000" };
        var resolver = new DeviceResolver("adb.exe", runner, settings, new[] { "100.106.31.127:38177" });

        var result = await resolver.ResolveAsync(CancellationToken.None);

        Assert.Equal("10.0.0.2:40000", result.Endpoint);
        Assert.DoesNotContain(runner.Calls, c => c.Contains("100.106.31.127:38177"));
    }

    private sealed class ScriptedRunner : IProcessRunner
    {
        private readonly Queue<(string token, ProcessResult result)> _steps = new();
        public List<string> Calls { get; } = new();
        public void Enqueue(string token, ProcessResult result) => _steps.Enqueue((token, result));
        public Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> args, CancellationToken ct)
        {
            var call = string.Join(' ', args);
            Calls.Add(call);
            var step = _steps.Dequeue();
            Assert.Contains(step.token, call);
            return Task.FromResult(step.result);
        }
    }
}
