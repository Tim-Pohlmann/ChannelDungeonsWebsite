using Bunit;
using ChannelDungeons.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class AppRouterTests : BunitContext
{
    [TestMethod]
    public void RootRoute_RendersHomeInsideMainLayout()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<App>();

        Assert.Contains("Channel Dungeons", cut.Find("h1").TextContent);
    }

    [TestMethod]
    public void UnknownRoute_RendersNotFoundMessage()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.GetRequiredService<NavigationManager>().NavigateTo("does-not-exist");

        var cut = Render<App>();

        Assert.Contains("nothing at this address", cut.Markup);
    }
}
