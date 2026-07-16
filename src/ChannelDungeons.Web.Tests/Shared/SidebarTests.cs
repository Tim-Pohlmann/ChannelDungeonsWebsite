using Bunit;
using ChannelDungeons.Web.Models;
using ChannelDungeons.Web.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Web.Tests.Shared;

[TestClass]
public class SidebarTests : Bunit.TestContext
{
    private static readonly List<ChannelInfo> Channels = new()
    {
        new() { Name = "welcome", Description = "d1" },
        new() { Name = "about", Description = "d2" },
    };

    [TestMethod]
    public void RendersOneEntryPerChannel()
    {
        var cut = RenderComponent<Sidebar>(p => p.Add(c => c.Channels, Channels));

        var items = cut.FindAll("li.channel");
        Assert.AreEqual(2, items.Count);
    }

    [TestMethod]
    public void MarksCurrentChannel_AsActive()
    {
        var cut = RenderComponent<Sidebar>(p => p
            .Add(c => c.Channels, Channels)
            .Add(c => c.CurrentChannel, "about"));

        var active = cut.Find("li.channel.active");
        Assert.IsTrue(active.TextContent.Contains("about"));
    }

    [TestMethod]
    public void AppliesVisibleClass_WhenVisibleParameterIsTrue()
    {
        var cut = RenderComponent<Sidebar>(p => p
            .Add(c => c.Channels, Channels)
            .Add(c => c.Visible, true));

        Assert.IsTrue(cut.Find("nav.sidebar").ClassList.Contains("visible"));
    }

    [TestMethod]
    public void ClickingChannel_InvokesOnSelect_WithChannelName()
    {
        string? selected = null;
        var cut = RenderComponent<Sidebar>(p => p
            .Add(c => c.Channels, Channels)
            .Add(c => c.OnSelect, name => selected = name));

        cut.FindAll("li.channel")[1].Click();

        Assert.AreEqual("about", selected);
    }
}
