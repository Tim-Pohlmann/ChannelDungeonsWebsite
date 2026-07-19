using Bunit;
using ChannelDungeons.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace ChannelDungeons.Web.Tests;

/// <summary>
/// bUnit context with the app's services registered, animation delays
/// replaced by an instant implementation, and browser interop replaced by a
/// no-op fake so tests run without waiting or JS.
/// </summary>
public abstract class AppTestContext : BunitContext
{
    protected AppTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IChannelCatalog, ChannelCatalog>();
        Services.AddSingleton<IDelayProvider, InstantDelayProvider>();
        Services.AddSingleton(TimeProvider.System);
        Services.AddSingleton<IBrowserInterop, FakeBrowserInterop>();
    }

    private sealed class InstantDelayProvider : IDelayProvider
    {
        public Task DelayAsync(int milliseconds, CancellationToken cancellationToken) =>
            cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : Task.CompletedTask;
    }

    /// <summary>No-op interop reporting a desktop viewport.</summary>
    private sealed class FakeBrowserInterop : IBrowserInterop
    {
        public Task<bool> InitAsync<T>(DotNetObjectReference<T> reference) where T : class =>
            Task.FromResult(false);

        public Task DisposeListenersAsync() => Task.CompletedTask;

        public Task ScrollToBottomAsync(ElementReference element) => Task.CompletedTask;

        public Task ScrollItemIntoViewAsync(ElementReference container, int index) => Task.CompletedTask;

        public Task RegisterInputKeysAsync(ElementReference input, ElementReference dropdown) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
