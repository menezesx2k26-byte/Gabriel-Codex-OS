using YouCineBridge;

namespace YouCineBridge.Tests;

public sealed class HotkeyControllerTests
{
    [Theory]
    [InlineData(1, true, BridgeHotkey.TogglePip)]
    [InlineData(2, true, BridgeHotkey.Focus)]
    [InlineData(3, true, BridgeHotkey.Reconnect)]
    [InlineData(4, true, BridgeHotkey.ToggleFullscreen)]
    public void ResolveAction_MapsRegisteredIds(int id, bool focused, BridgeHotkey expected)
    {
        Assert.Equal(expected, HotkeyController.ResolveAction(id, focused));
    }

    [Fact]
    public void ResolveAction_IgnoresF11WhenYouCineIsNotFocused()
    {
        Assert.Null(HotkeyController.ResolveAction(4, false));
    }
}
