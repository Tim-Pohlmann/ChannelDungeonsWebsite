using Bunit;
using ChannelDungeons.Web.Pages;
using ChannelDungeons.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class HomePageTests : AppTestContext
{
    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    private sealed class PendingDelayProvider : IDelayProvider
    {
        public Task DelayAsync(int milliseconds, CancellationToken cancellationToken) =>
            Task.Delay(Timeout.Infinite, cancellationToken);
    }

    [TestMethod]
    public void InitialLoad_AnimatesAllWelcomeMessages()
    {
        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            Assert.HasCount(4, cut.FindAll(".message"));
            Assert.Contains("Welcome to Channel Dungeons!", cut.Find(".message h1").TextContent);
        });
    }

    [TestMethod]
    public void InitialLoad_RevealsSidebarAndInputAfterAnimation()
    {
        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("visible", cut.Find(".sidebar").ClassList);
            Assert.Contains("visible", cut.Find(".command-input-container").ClassList);
        });
    }

    [TestMethod]
    public void SidebarClick_SwitchesToSelectedChannel()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.HasCount(4, cut.FindAll(".message")));

        cut.Find("[data-channel='features']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.AreEqual("features", cut.Find("#current-channel").TextContent);
            Assert.Contains("Gameplay Features", cut.Find(".channel-description").TextContent);
            Assert.HasCount(4, cut.FindAll(".message .feature"));
        });
    }

    [TestMethod]
    public void DirectChannelUrl_ShowsMessagesImmediatelyWithUiVisible()
    {
        Navigation.NavigateTo(Navigation.BaseUri + "#about");

        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            Assert.AreEqual("about", cut.Find("#current-channel").TextContent);
            Assert.HasCount(4, cut.FindAll(".message"));
            Assert.Contains("visible", cut.Find(".command-input-container").ClassList);
        });
    }

    [TestMethod]
    public void UnknownChannelUrl_FallsBackToWelcome()
    {
        Navigation.NavigateTo(Navigation.BaseUri + "#no-such-channel");

        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            Assert.AreEqual("welcome", cut.Find("#current-channel").TextContent);
            Assert.HasCount(4, cut.FindAll(".message"));
        });
    }

    [TestMethod]
    public void UnknownChannelUrl_TakesTheAnimatedWelcomePath()
    {
        // A delay that never completes freezes the animation at its first
        // step: the animated path shows no messages yet, while the
        // direct-link path would have shown all of them instantly.
        Services.AddSingleton<IDelayProvider>(new PendingDelayProvider());
        Navigation.NavigateTo(Navigation.BaseUri + "#no-such-channel");

        var cut = Render<Home>();

        Assert.IsEmpty(cut.FindAll(".message"));
        Assert.DoesNotContain("visible", cut.Find(".command-input-container").ClassList);
    }

    [TestMethod]
    public async Task ContentArea_ReservesSidebarWidthOnlyWhileSidebarVisible()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.Contains("sidebar-visible", cut.Find("main").ClassList));

        await cut.Instance.OnSwipe(isRightSwipe: false);

        cut.WaitForAssertion(() => Assert.DoesNotContain("sidebar-visible", cut.Find("main").ClassList));
    }

    [TestMethod]
    public void PlainTextInput_GetsBotReply()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.HasCount(4, cut.FindAll(".message")));

        cut.Find("#command-input").Input("hello");
        cut.Find("#command-input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        cut.WaitForAssertion(() =>
        {
            Assert.HasCount(5, cut.FindAll(".message"));
            Assert.Contains("demonstration of a Discord-like interface", cut.FindAll(".message-text")[^1].TextContent);
        });
    }

    [TestMethod]
    public void UnknownCommand_GetsBotReplyListingCommands()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.HasCount(4, cut.FindAll(".message")));

        cut.Find("#command-input").Input("/dance");
        cut.Find("#command-input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        cut.WaitForAssertion(() =>
            Assert.Contains("Unknown command: /dance", cut.FindAll(".message-text")[^1].TextContent));
    }

    [TestMethod]
    public void KnownCommand_SwitchesChannel()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.HasCount(4, cut.FindAll(".message")));

        // First Enter accepts the autocomplete suggestion, second Enter submits.
        cut.Find("#command-input").Input("/gameplay-demo");
        cut.Find("#command-input").KeyDown(new KeyboardEventArgs { Key = "Enter" });
        cut.Find("#command-input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        cut.WaitForAssertion(() =>
            Assert.AreEqual("gameplay-demo", cut.Find("#current-channel").TextContent));
    }

    [TestMethod]
    public void RevisitedChannel_KeepsItsMessages()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.HasCount(4, cut.FindAll(".message")));

        cut.Find("[data-channel='features']").Click();
        cut.WaitForAssertion(() => Assert.AreEqual("features", cut.Find("#current-channel").TextContent));

        cut.Find("[data-channel='welcome']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.AreEqual("welcome", cut.Find("#current-channel").TextContent);
            Assert.HasCount(4, cut.FindAll(".message"));
        });
    }

    [TestMethod]
    public async Task SwipeLeft_HidesSidebar()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.Contains("visible", cut.Find(".sidebar").ClassList));

        await cut.Instance.OnSwipe(isRightSwipe: false);

        cut.WaitForAssertion(() =>
            Assert.DoesNotContain("visible", cut.Find(".sidebar").ClassList));
    }

    [TestMethod]
    public async Task SwipeRight_ShowsSidebar()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.Contains("visible", cut.Find(".sidebar").ClassList));
        await cut.Instance.OnSwipe(isRightSwipe: false);

        await cut.Instance.OnSwipe(isRightSwipe: true);

        cut.WaitForAssertion(() =>
            Assert.Contains("visible", cut.Find(".sidebar").ClassList));
    }

    [TestMethod]
    public async Task ResizeToMobile_HidesSidebar()
    {
        var cut = Render<Home>();
        cut.WaitForAssertion(() => Assert.Contains("visible", cut.Find(".sidebar").ClassList));

        await cut.Instance.OnViewportResized(isMobile: true);

        cut.WaitForAssertion(() =>
            Assert.DoesNotContain("visible", cut.Find(".sidebar").ClassList));
    }
}
