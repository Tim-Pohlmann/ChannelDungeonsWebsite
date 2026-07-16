using Bunit;
using ChannelDungeons.Web.Pages;
using ChannelDungeons.Web.Services;
using ChannelDungeons.Web.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Web.Tests.Pages;

[TestClass]
public class HomeTests : Bunit.TestContext
{
    private Bunit.BunitJSModuleInterop SetUpModule(string? hash = null, bool isMobile = false)
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var module = JSInterop.SetupModule("./js/app.js");
        if (hash is not null)
            module.Setup<string>("getHash").SetResult(hash);
        module.Setup<bool>("isMobileViewport", 768).SetResult(isMobile);

        Services.AddScoped(_ => new BrowserInterop(JSInterop.JSRuntime));
        Services.AddSingleton<IDelayProvider>(new FakeDelayProvider());
        return module;
    }

    [TestMethod]
    public void WelcomeChannel_EventuallyRevealsSidebarAndCommandInput()
    {
        SetUpModule();

        var cut = RenderComponent<Home>();

        cut.WaitForState(() =>
                cut.Find(".sidebar").ClassList.Contains("visible") &&
                cut.Find(".command-input-container").ClassList.Contains("visible"),
            timeout: TimeSpan.FromSeconds(5));

        Assert.AreEqual(4, cut.FindAll(".message").Count);
    }

    [TestMethod]
    public void DirectHashToNonWelcomeChannel_LoadsImmediately_NoAnimation()
    {
        SetUpModule(hash: "features");

        var cut = RenderComponent<Home>();

        cut.WaitForState(() => cut.FindAll(".message").Count == 4, timeout: TimeSpan.FromSeconds(5));

        Assert.IsTrue(cut.Find(".sidebar").ClassList.Contains("visible"));
        Assert.IsTrue(cut.Find(".command-input-container").ClassList.Contains("visible"));
        Assert.IsFalse(cut.Find(".typing-indicator").ClassList.Contains("active"));
    }

    [TestMethod]
    public void ClickingSidebarToggle_TogglesSidebarVisibility()
    {
        SetUpModule(hash: "features");
        var cut = RenderComponent<Home>();
        cut.WaitForState(() => cut.FindAll(".message").Count == 4, timeout: TimeSpan.FromSeconds(5));
        Assert.IsTrue(cut.Find(".sidebar").ClassList.Contains("visible"));

        cut.Find(".sidebar-toggle").Click();

        Assert.IsFalse(cut.Find(".sidebar").ClassList.Contains("visible"));
    }

    [TestMethod]
    public void SelectingChannelFromSidebar_SwitchesChannel()
    {
        SetUpModule(hash: "features");
        var cut = RenderComponent<Home>();
        cut.WaitForState(() => cut.FindAll(".message").Count == 4, timeout: TimeSpan.FromSeconds(5));

        cut.Find(".channel:not(.active)").Click();

        cut.WaitForState(() => cut.Find(".channel-name span:last-child").TextContent != "features",
            timeout: TimeSpan.FromSeconds(5));
        Assert.AreNotEqual("features", cut.Find(".channel-name span:last-child").TextContent);
    }

    [TestMethod]
    public async Task SelectingChannelFromSidebar_OnMobile_ClosesSidebarImmediately()
    {
        SetUpModule(hash: "features", isMobile: true);
        var cut = RenderComponent<Home>();
        cut.WaitForState(() => cut.FindAll(".message").Count == 4, timeout: TimeSpan.FromSeconds(5));

        await cut.InvokeAsync(() => cut.Instance.OnWindowResized(true));
        cut.Find(".sidebar-toggle").Click();
        Assert.IsTrue(cut.Find(".sidebar").ClassList.Contains("visible"));

        cut.Find(".channel:not(.active)").Click();

        Assert.IsFalse(cut.Find(".sidebar").ClassList.Contains("visible"));
    }

    [TestMethod]
    public void SubmittingUnknownCommand_AppendsErrorMessage()
    {
        SetUpModule(hash: "features");
        var cut = RenderComponent<Home>();
        cut.WaitForState(() => cut.FindAll(".message").Count == 4, timeout: TimeSpan.FromSeconds(5));

        cut.Find("input#command-input").Input("/nope");
        cut.Find("input#command-input").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        cut.WaitForState(() => cut.FindAll(".message").Count == 5, timeout: TimeSpan.FromSeconds(5));
        Assert.IsTrue(cut.FindAll(".message")[4].TextContent.Contains("Unknown command"));
    }

    [TestMethod]
    public void SubmittingValidCommand_SwitchesChannel()
    {
        SetUpModule(hash: "features");
        var cut = RenderComponent<Home>();
        cut.WaitForState(() => cut.FindAll(".message").Count == 4, timeout: TimeSpan.FromSeconds(5));

        cut.Find("input#command-input").Input("/about");
        // First Enter selects the autocomplete match into the input; second submits it.
        cut.Find("input#command-input").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });
        cut.Find("input#command-input").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        cut.WaitForState(() => cut.Find(".channel-name span:last-child").TextContent == "about",
            timeout: TimeSpan.FromSeconds(5));
        Assert.AreEqual("about", cut.Find(".channel-name span:last-child").TextContent);
    }

    [TestMethod]
    public async Task OnSwipe_PastThreshold_ShowsSidebar()
    {
        SetUpModule(hash: "features", isMobile: true);
        var cut = RenderComponent<Home>();
        cut.WaitForState(() => cut.FindAll(".message").Count == 4, timeout: TimeSpan.FromSeconds(5));
        await cut.InvokeAsync(() => cut.Instance.OnWindowResized(true));
        Assert.IsFalse(cut.Find(".sidebar").ClassList.Contains("visible"));

        await cut.InvokeAsync(() => cut.Instance.OnSwipe(100));

        Assert.IsTrue(cut.Find(".sidebar").ClassList.Contains("visible"));
    }

    [TestMethod]
    public async Task OnSwipe_BelowThreshold_DoesNotShowSidebar()
    {
        SetUpModule(hash: "features", isMobile: true);
        var cut = RenderComponent<Home>();
        cut.WaitForState(() => cut.FindAll(".message").Count == 4, timeout: TimeSpan.FromSeconds(5));
        await cut.InvokeAsync(() => cut.Instance.OnWindowResized(true));

        await cut.InvokeAsync(() => cut.Instance.OnSwipe(10));

        Assert.IsFalse(cut.Find(".sidebar").ClassList.Contains("visible"));
    }
}
