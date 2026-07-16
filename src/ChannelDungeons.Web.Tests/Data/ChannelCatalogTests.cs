using ChannelDungeons.Web.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Web.Tests.Data;

[TestClass]
public class ChannelCatalogTests
{
    [TestMethod]
    public void DefaultChannel_IsAValidChannel()
    {
        Assert.IsTrue(ChannelCatalog.IsValidChannel(ChannelCatalog.DefaultChannel));
    }

    [TestMethod]
    public void Channels_AllHaveUniqueNames()
    {
        var names = ChannelCatalog.Channels.Select(c => c.Name).ToList();
        Assert.AreEqual(names.Count, names.Distinct(StringComparer.Ordinal).Count());
    }

    [TestMethod]
    public void Channels_AllHaveAtLeastOneMessage()
    {
        foreach (var channel in ChannelCatalog.Channels)
        {
            Assert.IsTrue(channel.Messages.Count > 0, $"Channel '{channel.Name}' should have at least one message");
        }
    }

    [TestMethod]
    public void Find_ReturnsChannel_ForExactCaseMatch()
    {
        var channel = ChannelCatalog.Find("about");
        Assert.IsNotNull(channel);
        Assert.AreEqual("about", channel!.Name);
    }

    [TestMethod]
    public void Find_ReturnsNull_ForDifferentCase()
    {
        Assert.IsNull(ChannelCatalog.Find("ABOUT"));
    }

    [TestMethod]
    public void Find_ReturnsNull_ForUnknownChannel()
    {
        Assert.IsNull(ChannelCatalog.Find("does-not-exist"));
    }

    [TestMethod]
    public void IsValidChannel_ReturnsFalse_ForUnknownChannel()
    {
        Assert.IsFalse(ChannelCatalog.IsValidChannel("does-not-exist"));
    }

    [TestMethod]
    public void InSidebarOrder_PutsDefaultChannelFirst()
    {
        var ordered = ChannelCatalog.InSidebarOrder().ToList();
        Assert.AreEqual(ChannelCatalog.DefaultChannel, ordered[0].Name);
    }

    [TestMethod]
    public void InSidebarOrder_OrdersRemainingChannelsAlphabetically()
    {
        var rest = ChannelCatalog.InSidebarOrder().Skip(1).Select(c => c.Name).ToList();
        var expected = rest.OrderBy(n => n, StringComparer.Ordinal).ToList();
        CollectionAssert.AreEqual(expected, rest);
    }

    [TestMethod]
    public void InSidebarOrder_ContainsAllChannels()
    {
        Assert.AreEqual(ChannelCatalog.Channels.Count, ChannelCatalog.InSidebarOrder().Count());
    }
}
