using Bunit;
using ChannelDungeons.Web.Pages;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class HomePageTests : BunitContext
{
    [TestMethod]
    public void Home_RendersSiteHeading()
    {
        var cut = Render<Home>();

        var heading = cut.Find("h1");
        Assert.AreEqual("Channel Dungeons", heading.TextContent);
    }
}
