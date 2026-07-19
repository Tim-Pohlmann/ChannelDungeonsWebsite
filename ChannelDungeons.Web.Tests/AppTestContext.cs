using Bunit;
using ChannelDungeons.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChannelDungeons.Web.Tests;

/// <summary>
/// bUnit context with the app's services registered and animation delays
/// replaced by an instant implementation so tests run without waiting.
/// </summary>
public abstract class AppTestContext : BunitContext
{
    protected AppTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IChannelCatalog, ChannelCatalog>();
        Services.AddSingleton<IDelayProvider, InstantDelayProvider>();
        Services.AddSingleton(TimeProvider.System);
    }

    private sealed class InstantDelayProvider : IDelayProvider
    {
        public Task DelayAsync(int milliseconds, CancellationToken cancellationToken) =>
            cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : Task.CompletedTask;
    }
}
