using ChannelDungeons.Web.Models;
using ChannelDungeons.Web.Services;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class CommandProcessorTests
{
    private static readonly IReadOnlyList<CommandSuggestion> Commands =
    [
        new("welcome", "Welcome to Channel Dungeons!"),
        new("about", "More about Channel Dungeons")
    ];

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void EmptyOrWhitespaceInput_ReturnsNone(string input)
    {
        var result = CommandProcessor.Process(input, Commands);

        Assert.AreEqual(CommandResultKind.None, result.Kind);
    }

    [TestMethod]
    public void PlainText_ReturnsDemoReply()
    {
        var result = CommandProcessor.Process("hello there", Commands);

        Assert.AreEqual(CommandResultKind.BotReply, result.Kind);
        StringAssert.Contains(result.ReplyHtml, "demonstration of a Discord-like interface");
    }

    [TestMethod]
    public void KnownCommand_SwitchesChannel()
    {
        var result = CommandProcessor.Process("/about", Commands);

        Assert.AreEqual(CommandResultKind.SwitchChannel, result.Kind);
        Assert.AreEqual("about", result.ChannelId);
    }

    [TestMethod]
    public void KnownCommandInMixedCase_SwitchesChannel()
    {
        var result = CommandProcessor.Process("/About", Commands);

        Assert.AreEqual(CommandResultKind.SwitchChannel, result.Kind);
        Assert.AreEqual("about", result.ChannelId);
    }

    [TestMethod]
    public void SurroundingWhitespace_IsIgnored()
    {
        var result = CommandProcessor.Process("  /welcome  ", Commands);

        Assert.AreEqual(CommandResultKind.SwitchChannel, result.Kind);
        Assert.AreEqual("welcome", result.ChannelId);
    }

    [TestMethod]
    public void UnknownCommand_ListsAvailableCommands()
    {
        var result = CommandProcessor.Process("/dance", Commands);

        Assert.AreEqual(CommandResultKind.BotReply, result.Kind);
        StringAssert.Contains(result.ReplyHtml, "Unknown command: /dance");
        // No literal slash inside the spans: .discord-command adds it via CSS.
        StringAssert.Contains(result.ReplyHtml, "<span class='discord-command'>welcome</span>");
        StringAssert.Contains(result.ReplyHtml, "<span class='discord-command'>about</span>");
    }

    [TestMethod]
    public void UnknownCommand_HtmlEncodesEchoedInput()
    {
        var result = CommandProcessor.Process("/<img onerror=x>", Commands);

        Assert.AreEqual(CommandResultKind.BotReply, result.Kind);
        StringAssert.Contains(result.ReplyHtml, "&lt;img onerror=x&gt;");
        Assert.DoesNotContain("<img", result.ReplyHtml);
    }
}
