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
    /// HTML markup content of the message, rendered via <c>MarkupString</c> in MessageDisplay.
    /// Callers must ensure all user-supplied values are HTML-encoded before embedding.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Duration in milliseconds to show typing indicator before message appears.
    /// If null, uses default from config.
    /// </summary>
    public int? TypingDuration { get; set; }

    /// <summary>
    /// Extra pre-delay in milliseconds added on top of the configured between-message gap
    /// before showing this message's typing indicator. Additive: does not replace the
    /// default gap, it extends it. If null, no extra delay is applied.
    /// </summary>
    public int? Delay { get; set; }

    /// <summary>
    /// Timestamp generated at runtime. Set by animation service.
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;
}
