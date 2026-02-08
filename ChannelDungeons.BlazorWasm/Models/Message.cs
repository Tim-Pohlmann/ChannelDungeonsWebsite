namespace ChannelDungeons.BlazorWasm.Models;

/// <summary>
/// Represents a single message in a channel.
/// </summary>
public class Message
{
    /// <summary>
    /// Username of the message sender (typically "Channel Dungeons").
    /// </summary>
    public string Username { get; set; } = "Channel Dungeons";

    /// <summary>
    /// HTML content of the message. Should be sanitized on load.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Duration in milliseconds to show typing indicator before message appears.
    /// If null, uses default from config.
    /// </summary>
    public int? TypingDuration { get; set; }

    /// <summary>
    /// Delay in milliseconds before showing typing indicator for next message.
    /// If null, uses default from config.
    /// </summary>
    public int? Delay { get; set; }

    /// <summary>
    /// Timestamp generated at runtime. Set by animation service.
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;
}
