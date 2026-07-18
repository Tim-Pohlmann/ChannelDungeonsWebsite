using ChannelDungeons.Web.Models;
using ChannelDungeons.Web.Services;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class MessageScheduleTests
{
    [TestMethod]
    public void MessageWithoutTimings_UsesDefaults()
    {
        var schedule = MessageSchedule.Build([new ChannelMessage("<p>hi</p>")]);

        Assert.HasCount(1, schedule);
        Assert.AreEqual(0, schedule[0].WaitBeforeTypingMs);
        Assert.AreEqual(MessageSchedule.DefaultTypingDurationMs, schedule[0].TypingDurationMs);
    }

    [TestMethod]
    public void FirstMessage_WaitsOnlyItsOwnDelay()
    {
        var schedule = MessageSchedule.Build(
            [new ChannelMessage("<p>hi</p>", TypingDurationMs: 800, DelayMs: 300)]);

        Assert.AreEqual(300, schedule[0].WaitBeforeTypingMs);
        Assert.AreEqual(800, schedule[0].TypingDurationMs);
    }

    [TestMethod]
    public void SubsequentMessages_AddTheBetweenMessagesGap()
    {
        var schedule = MessageSchedule.Build(
        [
            new ChannelMessage("<p>1</p>", TypingDurationMs: 800, DelayMs: 300),
            new ChannelMessage("<p>2</p>", TypingDurationMs: 700, DelayMs: 1200)
        ]);

        Assert.AreEqual(MessageSchedule.DelayBetweenMessagesMs + 1200, schedule[1].WaitBeforeTypingMs);
        Assert.AreEqual(700, schedule[1].TypingDurationMs);
    }

    [TestMethod]
    public void SubsequentMessageWithoutDelay_WaitsOnlyTheGap()
    {
        var schedule = MessageSchedule.Build(
        [
            new ChannelMessage("<p>1</p>"),
            new ChannelMessage("<p>2</p>")
        ]);

        Assert.AreEqual(MessageSchedule.DelayBetweenMessagesMs, schedule[1].WaitBeforeTypingMs);
    }
}
