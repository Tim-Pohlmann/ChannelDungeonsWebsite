using ChannelDungeons.Web.Services;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class ChannelCatalogTests
{
    private static readonly ChannelCatalog Catalog = new();

    [TestMethod]
    public void Channels_ListWelcomeFirstThenAlphabetical()
    {
        var ids = Catalog.Channels.Select(c => c.Id).ToArray();

        CollectionAssert.AreEqual(
            new[] { "welcome", "about", "features", "gameplay-demo" }, ids);
    }

    [TestMethod]
    public void EveryChannel_HasDescriptionAndMessages()
    {
        foreach (var channel in Catalog.Channels)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(channel.Description), $"{channel.Id} has no description");
            Assert.IsNotEmpty(channel.Messages, $"{channel.Id} has no messages");
        }
    }

    [TestMethod]
    public void Commands_MirrorChannels()
    {
        CollectionAssert.AreEqual(
            Catalog.Channels.Select(c => c.Id).ToArray(),
            Catalog.Commands.Select(c => c.Name).ToArray());
    }

    [TestMethod]
    public void TryGetChannel_FindsExistingChannel()
    {
        Assert.IsTrue(Catalog.TryGetChannel("about", out var channel));
        Assert.AreEqual("about", channel.Id);
    }

    [TestMethod]
    public void TryGetChannel_RejectsUnknownChannel()
    {
        Assert.IsFalse(Catalog.TryGetChannel("no-such-channel", out _));
    }
}
