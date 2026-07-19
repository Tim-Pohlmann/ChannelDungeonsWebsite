using Bunit;
using ChannelDungeons.Web.Components;
using ChannelDungeons.Web.Models;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class SidebarTests : AppTestContext
{
    private static readonly IReadOnlyList<Channel> Channels =
    [
        new("welcome", "Welcome!", []),
        new("about", "About", [])
    ];

    [TestMethod]
    public void RendersAllChannels_AndHighlightsTheCurrentOne()
    {
        var cut = Render<Sidebar>(parameters => parameters
            .Add(p => p.Channels, Channels)
            .Add(p => p.CurrentChannelId, "about")
            .Add(p => p.Visible, true));

        var items = cut.FindAll(".channel");
        Assert.HasCount(2, items);
        Assert.DoesNotContain("active", items[0].ClassList);
        Assert.Contains("active", items[1].ClassList);
        Assert.Contains("visible", cut.Find("nav").ClassList);
    }

    [TestMethod]
    public void ClickingChannel_RaisesSelectionCallback()
    {
        string? selected = null;
        var cut = Render<Sidebar>(parameters => parameters
            .Add(p => p.Channels, Channels)
            .Add(p => p.OnChannelSelected, id => selected = id));

        cut.Find("[data-channel='about']").Click();

        Assert.AreEqual("about", selected);
    }
}
