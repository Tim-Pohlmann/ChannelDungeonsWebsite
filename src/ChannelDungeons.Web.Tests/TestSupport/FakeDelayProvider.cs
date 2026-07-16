using ChannelDungeons.Web.Services;

namespace ChannelDungeons.Web.Tests.TestSupport;

/// <summary>Collapses animation timing to ~instant while still honoring cancellation, so bUnit tests don't wait on real message/typing delays.</summary>
public sealed class FakeDelayProvider : IDelayProvider
{
    public Task Delay(int milliseconds, CancellationToken cancellationToken = default) =>
        Task.Delay(1, cancellationToken);
}
