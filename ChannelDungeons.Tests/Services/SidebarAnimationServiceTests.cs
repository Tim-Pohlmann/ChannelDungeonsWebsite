using ChannelDungeons.BlazorWasm.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Tests.Services;

[TestClass]
public class SidebarAnimationServiceTests
{
    // --- Welcome channel, not yet revealed ---

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueForLastMessage_OnWelcomeChannel()
    {
        Assert.IsTrue(SidebarAnimationService.ShouldShowSidebar(2, 3, "welcome", false));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForFirstMessage_OnWelcomeChannel()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(0, 3, "welcome", false));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForMiddleMessage_OnWelcomeChannel()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(1, 3, "welcome", false));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueForSingleMessage_OnWelcomeChannel()
    {
        Assert.IsTrue(SidebarAnimationService.ShouldShowSidebar(0, 1, "welcome", false));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForZeroMessages()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(0, 0, "welcome", false));
    }

    // --- Reveal already done ---

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalse_WhenSidebarAlreadyRevealed()
    {
        // Even on the last message of the welcome channel, don't show again
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(2, 3, "welcome", true));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalse_SingleMessageWelcomeAlreadyRevealed()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(0, 1, "welcome", true));
    }

    // --- Non-welcome channels ---

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalse_ForNonWelcomeChannel_LastMessage()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(2, 3, "about", false));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalse_ForFeaturesChannel()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(3, 4, "features", false));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalse_ForGameplayDemoChannel()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(0, 1, "gameplay-demo", false));
    }

    [TestMethod]
    public void ShouldShowSidebar_IsCaseInsensitive_ForWelcomeChannelId()
    {
        Assert.IsTrue(SidebarAnimationService.ShouldShowSidebar(0, 1, "WELCOME", false));
        Assert.IsTrue(SidebarAnimationService.ShouldShowSidebar(0, 1, "Welcome", false));
    }

    // --- Full sequence check ---

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueOnlyForExactLastIndex_OnWelcomeChannel()
    {
        const int totalMessages = 10;

        for (int i = 0; i < totalMessages; i++)
        {
            var result = SidebarAnimationService.ShouldShowSidebar(i, totalMessages, "welcome", false);
            if (i == totalMessages - 1)
                Assert.IsTrue(result, $"Should return true at last index {totalMessages - 1}");
            else
                Assert.IsFalse(result, $"Should return false at index {i}");
        }
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueWhenIndexEqualsLargeMessageCount()
    {
        Assert.IsTrue(SidebarAnimationService.ShouldShowSidebar(999, 1000, "welcome", false));
    }

    [TestMethod]
    [DataRow(0, 5)]
    [DataRow(1, 5)]
    [DataRow(2, 5)]
    [DataRow(3, 5)]
    public void ShouldShowSidebar_ReturnsFalseForAllButLastIndex_OnWelcomeChannel(int messageIndex, int totalMessages)
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(messageIndex, totalMessages, "welcome", false));
    }
}
