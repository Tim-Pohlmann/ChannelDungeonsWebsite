namespace ChannelDungeons.Web.Services;

/// <summary>Abstraction over Task.Delay so animation timing can be replaced with a fast, deterministic implementation in tests.</summary>
public interface IDelayProvider
{
    Task Delay(int milliseconds, CancellationToken cancellationToken = default);
}
