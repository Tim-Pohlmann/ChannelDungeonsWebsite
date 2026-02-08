namespace ChannelDungeons.BlazorWasm.Models;

/// <summary>
/// Represents a Discord-like channel in the application.
/// </summary>
public class Channel
{
    /// <summary>
    /// Unique identifier for the channel (used in URLs).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the channel.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description shown in the channel header.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Collection of messages to display in this channel.
    /// </summary>
    public List<Message> Messages { get; set; } = new();
}
