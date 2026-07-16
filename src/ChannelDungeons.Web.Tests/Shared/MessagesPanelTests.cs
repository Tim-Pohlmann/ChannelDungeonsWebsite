using Bunit;
using ChannelDungeons.Web.Models;
using ChannelDungeons.Web.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Web.Tests.Shared;

[TestClass]
public class MessagesPanelTests : Bunit.TestContext
{
    [TestMethod]
    public void RendersOneEntryPerMessage()
    {
        var messages = new List<DisplayedMessage>
        {
            new(new ChannelMessage { ContentHtml = "<p>a</p>" }, "1:00 PM"),
            new(new ChannelMessage { ContentHtml = "<p>b</p>" }, "1:01 PM"),
        };

        var cut = RenderComponent<MessagesPanel>(p => p.Add(c => c.Messages, messages));

        Assert.AreEqual(2, cut.FindAll("div.message").Count);
    }

    [TestMethod]
    public void RendersMessageContent_AsMarkup()
    {
        var messages = new List<DisplayedMessage>
        {
            new(new ChannelMessage { ContentHtml = "<p>hello <strong>world</strong></p>" }, "1:00 PM"),
        };

        var cut = RenderComponent<MessagesPanel>(p => p.Add(c => c.Messages, messages));

        Assert.IsNotNull(cut.Find("strong"));
    }

    [TestMethod]
    public void TypingIndicator_HasActiveClass_WhenVisible()
    {
        var cut = RenderComponent<MessagesPanel>(p => p.Add(c => c.TypingIndicatorVisible, true));

        Assert.IsTrue(cut.Find("div.typing-indicator").ClassList.Contains("active"));
    }

    [TestMethod]
    public void TypingIndicator_HasNoActiveClass_WhenHidden()
    {
        var cut = RenderComponent<MessagesPanel>(p => p.Add(c => c.TypingIndicatorVisible, false));

        Assert.IsFalse(cut.Find("div.typing-indicator").ClassList.Contains("active"));
    }
}
