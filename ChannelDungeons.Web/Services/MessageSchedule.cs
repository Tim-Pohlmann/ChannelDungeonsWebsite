using ChannelDungeons.Web.Models;

namespace ChannelDungeons.Web.Services;

/// <summary>One step of a channel's message animation.</summary>
/// <param name="WaitBeforeTypingMs">Pause before the typing indicator appears.</param>
/// <param name="TypingDurationMs">How long the typing indicator shows before the message.</param>
public sealed record ScheduledMessage(
    ChannelMessage Message,
    int WaitBeforeTypingMs,
    int TypingDurationMs);

/// <summary>
/// Computes the message animation timeline, mirroring the original site:
/// each message waits its own delay, shows typing for its duration, then a
/// fixed gap separates it from the next message.
/// </summary>
public static class MessageSchedule
{
    public const int DefaultTypingDurationMs = 1000;
    public const int DelayBetweenMessagesMs = 200;
    public const int UiRevealDelayMs = 500;

    public static IReadOnlyList<ScheduledMessage> Build(IReadOnlyList<ChannelMessage> messages)
    {
        var schedule = new List<ScheduledMessage>(messages.Count);
        for (var i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            var waitBeforeTyping = message.DelayMs ?? 0;
            if (i > 0)
            {
                waitBeforeTyping += DelayBetweenMessagesMs;
            }

            schedule.Add(new ScheduledMessage(
                message,
                waitBeforeTyping,
                message.TypingDurationMs ?? DefaultTypingDurationMs));
        }

        return schedule;
    }
}
