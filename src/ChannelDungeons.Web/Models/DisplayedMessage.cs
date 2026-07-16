namespace ChannelDungeons.Web.Models;

/// <summary>A channel message with the timestamp it was rendered at, cached so revisits don't re-stamp it.</summary>
public sealed record DisplayedMessage(ChannelMessage Message, string Timestamp);
