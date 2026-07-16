namespace ChannelDungeons.Web.Models;

public sealed class ChannelMessage
{
    public required string ContentHtml { get; init; }

    public string Username { get; init; } = "Channel Dungeons";

    /// <summary>How long the typing indicator shows before this message appears.</summary>
    public int TypingDurationMs { get; init; } = 1000;

    /// <summary>Extra pause before the typing indicator for this message appears.</summary>
    public int DelayMs { get; init; }
}
