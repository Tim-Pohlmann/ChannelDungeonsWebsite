using Bunit;
using ChannelDungeons.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class AppRouterTests : AppTestContext
{
    [TestMethod]
    public void RootRoute_RendersHomeInsideMainLayout()
    {
        var cut = Render<App>();

        cut.WaitForAssertion(() =>
            Assert.Contains("Welcome to Channel Dungeons!", cut.Find(".message h1").TextContent));
    }

    [TestMethod]
    public void UnknownRoute_RendersNotFoundMessage()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("does-not-exist");

        var cut = Render<App>();

        Assert.Contains("nothing at this address", cut.Markup);
    }
}
