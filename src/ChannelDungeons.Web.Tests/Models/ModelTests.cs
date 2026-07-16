using ChannelDungeons.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Web.Tests.Models;

[TestClass]
public class ModelTests
{
    [TestMethod]
    public void ChannelMessage_DefaultsUsername_ToChannelDungeons()
    {
        var message = new ChannelMessage { ContentHtml = "<p>hi</p>" };
        Assert.AreEqual("Channel Dungeons", message.Username);
    }

    [TestMethod]
    public void ChannelMessage_DefaultsTypingDuration_To1000Ms()
    {
        var message = new ChannelMessage { ContentHtml = "<p>hi</p>" };
        Assert.AreEqual(1000, message.TypingDurationMs);
    }

    [TestMethod]
    public void ChannelMessage_DefaultsDelay_ToZero()
    {
        var message = new ChannelMessage { ContentHtml = "<p>hi</p>" };
        Assert.AreEqual(0, message.DelayMs);
    }

    [TestMethod]
    public void ChannelInfo_DefaultsMessages_ToEmpty()
    {
        var info = new ChannelInfo { Name = "x", Description = "y" };
        Assert.AreEqual(0, info.Messages.Count);
    }

    [TestMethod]
    public void DisplayedMessage_EqualsByValue()
    {
        var message = new ChannelMessage { ContentHtml = "<p>hi</p>" };
        var a = new DisplayedMessage(message, "1:00 PM");
        var b = new DisplayedMessage(message, "1:00 PM");
        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void DisplayedMessage_DiffersByTimestamp()
    {
        var message = new ChannelMessage { ContentHtml = "<p>hi</p>" };
        var a = new DisplayedMessage(message, "1:00 PM");
        var b = new DisplayedMessage(message, "1:01 PM");
        Assert.AreNotEqual(a, b);
    }
}
