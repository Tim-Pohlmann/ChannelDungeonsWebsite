namespace ChannelDungeons.Web.Models;

/// <summary>A message as displayed in the message list.</summary>
public sealed record RenderedMessage(
    string Username,
    string Timestamp,
    string Html);
