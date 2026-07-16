namespace ChannelDungeons.Web.Models;

public sealed class ChannelInfo
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public IReadOnlyList<ChannelMessage> Messages { get; init; } = Array.Empty<ChannelMessage>();
}
