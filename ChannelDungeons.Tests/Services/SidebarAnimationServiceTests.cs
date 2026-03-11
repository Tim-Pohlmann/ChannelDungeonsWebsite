using ChannelDungeons.BlazorWasm.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Tests.Services;

[TestClass]
public class SidebarAnimationServiceTests
{
    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueForLastMessage()
    {
        Assert.IsTrue(SidebarAnimationService.ShouldShowSidebar(2, 3));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForFirstMessage()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(0, 3));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForMiddleMessage()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(1, 3));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueForSingleMessage()
    {
        Assert.IsTrue(SidebarAnimationService.ShouldShowSidebar(0, 1));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForZeroMessages()
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(0, 0));
    }

    [TestMethod]
    [DataRow(0, 5)]
    [DataRow(1, 5)]
    [DataRow(2, 5)]
    [DataRow(3, 5)]
    public void ShouldShowSidebar_ReturnsFalseForAllButLastIndex(int messageIndex, int totalMessages)
    {
        Assert.IsFalse(SidebarAnimationService.ShouldShowSidebar(messageIndex, totalMessages));
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueOnlyForExactLastIndex()
    {
        const int totalMessages = 10;

        for (int i = 0; i < totalMessages; i++)
        {
            var result = SidebarAnimationService.ShouldShowSidebar(i, totalMessages);
            if (i == totalMessages - 1)
                Assert.IsTrue(result, $"Should return true at last index {totalMessages - 1}");
            else
                Assert.IsFalse(result, $"Should return false at index {i}");
        }
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueWhenIndexEqualsLargeMessageCount()
    {
        Assert.IsTrue(SidebarAnimationService.ShouldShowSidebar(999, 1000));
    }
}
