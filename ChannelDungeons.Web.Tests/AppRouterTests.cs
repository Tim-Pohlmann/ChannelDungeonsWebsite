using Bunit;
using ChannelDungeons.Web;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class AppRouterTests : AppTestContext
{
    [TestMethod]
    public void RootRoute_RendersHomeInsideMainLayout()
    {
        var cut = Render<App>();

        cut.WaitForAssertion(() =>
            StringAssert.Contains(cut.Find(".message h1").TextContent, "Welcome to Channel Dungeons!"));
    }
}
