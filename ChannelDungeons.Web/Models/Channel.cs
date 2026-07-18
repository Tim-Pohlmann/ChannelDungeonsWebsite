namespace ChannelDungeons.Web.Models;

public sealed record Channel(
    string Id,
    string Description,
    IReadOnlyList<ChannelMessage> Messages);
