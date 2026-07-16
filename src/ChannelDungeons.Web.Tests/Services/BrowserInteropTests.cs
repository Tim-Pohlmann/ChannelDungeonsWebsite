using Bunit;
using ChannelDungeons.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Web.Tests.Services;

[TestClass]
public class BrowserInteropTests : Bunit.TestContext
{
    [TestMethod]
    public async Task GetHashAsync_ReturnsValueFromModule()
    {
        var module = JSInterop.SetupModule("./js/app.js");
        module.Setup<string>("getHash").SetResult("about");

        var interop = new BrowserInterop(JSInterop.JSRuntime);

        Assert.AreEqual("about", await interop.GetHashAsync());
    }

    [TestMethod]
    public async Task SetHashAsync_InvokesModuleWithValue()
    {
        var module = JSInterop.SetupModule("./js/app.js");
        var invocation = module.SetupVoid("setHash", "features").SetVoidResult();

        var interop = new BrowserInterop(JSInterop.JSRuntime);
        await interop.SetHashAsync("features");

        invocation.VerifyInvoke("setHash");
    }

    [TestMethod]
    public async Task IsMobileViewportAsync_ReturnsValueFromModule()
    {
        var module = JSInterop.SetupModule("./js/app.js");
        module.Setup<bool>("isMobileViewport", 768).SetResult(true);

        var interop = new BrowserInterop(JSInterop.JSRuntime);

        Assert.IsTrue(await interop.IsMobileViewportAsync(768));
    }

    [TestMethod]
    public async Task DisposeAsync_DoesNotThrow_WhenModuleWasNeverImported()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var interop = new BrowserInterop(JSInterop.JSRuntime);

        await interop.DisposeAsync();
    }

    [TestMethod]
    public async Task DisposeAsync_UnregistersListeners_WhenModuleWasImported()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./js/app.js");

        var interop = new BrowserInterop(JSInterop.JSRuntime);
        await interop.GetHashAsync();

        await interop.DisposeAsync();
    }
}
