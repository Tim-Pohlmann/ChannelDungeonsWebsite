using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ChannelDungeons.Web.Services;

/// <summary>
/// Wrapper around the wwwroot/app.js module so components depend on typed
/// methods instead of raw JS invocations.
/// </summary>
public interface IBrowserInterop : IAsyncDisposable
{
    /// <summary>
    /// Registers viewport/swipe listeners calling back into the given object.
    /// Returns whether the viewport is currently mobile-sized.
    /// </summary>
    Task<bool> InitAsync<T>(DotNetObjectReference<T> reference) where T : class;

    /// <summary>Detaches the listeners registered by <see cref="InitAsync{T}"/>.</summary>
    Task DisposeListenersAsync();

    Task ScrollToBottomAsync(ElementReference element);

    Task ScrollItemIntoViewAsync(ElementReference container, int index);

    Task RegisterInputKeysAsync(ElementReference input, ElementReference dropdown);
}

public sealed class BrowserInterop(IJSRuntime jsRuntime) : IBrowserInterop
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(
        () => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./app.js").AsTask());

    private Task<IJSObjectReference> ModuleAsync() => _moduleTask.Value;

    public async Task<bool> InitAsync<T>(DotNetObjectReference<T> reference) where T : class =>
        await (await ModuleAsync()).InvokeAsync<bool>("init", reference);

    public async Task DisposeListenersAsync() =>
        await (await ModuleAsync()).InvokeVoidAsync("dispose");

    public async Task ScrollToBottomAsync(ElementReference element) =>
        await (await ModuleAsync()).InvokeVoidAsync("scrollToBottom", element);

    public async Task ScrollItemIntoViewAsync(ElementReference container, int index) =>
        await (await ModuleAsync()).InvokeVoidAsync("scrollItemIntoView", container, index);

    public async Task RegisterInputKeysAsync(ElementReference input, ElementReference dropdown) =>
        await (await ModuleAsync()).InvokeVoidAsync("registerInputKeys", input, dropdown);

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
