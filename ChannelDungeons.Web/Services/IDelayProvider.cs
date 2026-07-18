namespace ChannelDungeons.Web.Services;

/// <summary>Abstracts awaitable delays so tests can run animations instantly.</summary>
public interface IDelayProvider
{
    Task DelayAsync(int milliseconds, CancellationToken cancellationToken);
}

public sealed class TaskDelayProvider : IDelayProvider
{
    public Task DelayAsync(int milliseconds, CancellationToken cancellationToken) =>
        Task.Delay(milliseconds, cancellationToken);
}
