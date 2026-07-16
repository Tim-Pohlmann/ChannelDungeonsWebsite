using Microsoft.JSInterop;

namespace ChannelDungeons.Web.Services;

/// <summary>
/// Thin wrapper around the wwwroot/js/app.js module. Centralizes JS interop so
/// components don't each import the module separately.
/// </summary>
public sealed class BrowserInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public BrowserInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new(() => _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/app.js").AsTask());
    }

    private async Task<IJSObjectReference> ModuleAsync() => await _moduleTask.Value;

    public async Task<string> GetHashAsync() => await (await ModuleAsync()).InvokeAsync<string>("getHash");

    public async Task SetHashAsync(string value) => await (await ModuleAsync()).InvokeVoidAsync("setHash", value);

    public async Task ScrollToBottomAsync(string elementId) =>
        await (await ModuleAsync()).InvokeVoidAsync("scrollToBottom", elementId);

    public async Task ScrollIntoViewAsync(string elementId) =>
        await (await ModuleAsync()).InvokeVoidAsync("scrollIntoView", elementId);

    public async Task FocusAsync(string elementId) => await (await ModuleAsync()).InvokeVoidAsync("focusElement", elementId);

    public async Task<bool> IsMobileViewportAsync(int breakpointPx) =>
        await (await ModuleAsync()).InvokeAsync<bool>("isMobileViewport", breakpointPx);

    public async Task RegisterAppListenersAsync<T>(DotNetObjectReference<T> reference, int breakpointPx) where T : class =>
        await (await ModuleAsync()).InvokeVoidAsync("registerAppListeners", reference, breakpointPx);

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("unregisterAppListeners");
            await module.DisposeAsync();
        }
    }
}
