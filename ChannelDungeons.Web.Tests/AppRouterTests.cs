using Bunit;
using ChannelDungeons.Web;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class AppRouterTests : BunitContext
{
    [TestMethod]
    public void RootRoute_RendersHomeInsideMainLayout()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<App>();

        StringAssert.Contains(cut.Find("h1").TextContent, "Channel Dungeons");
    }
}
