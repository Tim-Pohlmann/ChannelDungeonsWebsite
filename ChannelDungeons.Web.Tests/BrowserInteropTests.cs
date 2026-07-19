using Bunit;
using ChannelDungeons.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class BrowserInteropTests : BunitContext
{
    private BunitJSModuleInterop SetUpModule()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        return JSInterop.SetupModule("./app.js");
    }

    [TestMethod]
    public async Task InitAsync_ReturnsViewportKindFromModule()
    {
        var module = SetUpModule();
        module.Setup<bool>("init", _ => true).SetResult(true);
        await using var interop = new BrowserInterop(JSInterop.JSRuntime);
        using var reference = DotNetObjectReference.Create(this);

        var isMobile = await interop.InitAsync(reference);

        Assert.IsTrue(isMobile);
    }

    [TestMethod]
    public async Task ScrollToBottomAsync_InvokesModuleFunction()
    {
        var module = SetUpModule();
        await using var interop = new BrowserInterop(JSInterop.JSRuntime);

        await interop.ScrollToBottomAsync(default);

        Assert.HasCount(1, module.Invocations["scrollToBottom"]);
    }

    [TestMethod]
    public async Task ScrollItemIntoViewAsync_PassesIndex()
    {
        var module = SetUpModule();
        await using var interop = new BrowserInterop(JSInterop.JSRuntime);

        await interop.ScrollItemIntoViewAsync(default, 2);

        var invocation = module.Invocations["scrollItemIntoView"].Single();
        Assert.AreEqual(2, invocation.Arguments[1]);
    }

    [TestMethod]
    public async Task RegisterInputKeysAsync_InvokesModuleFunction()
    {
        var module = SetUpModule();
        await using var interop = new BrowserInterop(JSInterop.JSRuntime);

        await interop.RegisterInputKeysAsync(default, default);

        Assert.HasCount(1, module.Invocations["registerInputKeys"]);
    }

    [TestMethod]
    public async Task DisposeListenersAsync_InvokesModuleFunction()
    {
        var module = SetUpModule();
        await using var interop = new BrowserInterop(JSInterop.JSRuntime);

        await interop.DisposeListenersAsync();

        Assert.HasCount(1, module.Invocations["dispose"]);
    }

    [TestMethod]
    public async Task DisposeAsync_WithoutModuleUse_DoesNotImportModule()
    {
        var module = SetUpModule();
        var interop = new BrowserInterop(JSInterop.JSRuntime);

        await interop.DisposeAsync();

        Assert.IsEmpty(module.Invocations);
    }
}
