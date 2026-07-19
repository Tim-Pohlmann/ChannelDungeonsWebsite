namespace ChannelDungeons.Web.Models;

/// <summary>
/// A single authored message in a channel. <paramref name="Html"/> is trusted,
/// compiled-in markup (the site's content), never user input.
/// </summary>
public sealed record ChannelMessage(
    string Html,
    int? TypingDurationMs = null,
    int? DelayMs = null);
