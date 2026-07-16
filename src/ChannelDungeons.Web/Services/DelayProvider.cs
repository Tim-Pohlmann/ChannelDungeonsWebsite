namespace ChannelDungeons.Web.Services;

public sealed class DelayProvider : IDelayProvider
{
    public Task Delay(int milliseconds, CancellationToken cancellationToken = default) =>
        Task.Delay(milliseconds, cancellationToken);
}
