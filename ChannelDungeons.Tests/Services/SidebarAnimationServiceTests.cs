using ChannelDungeons.BlazorWasm.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Tests.Services;

[TestClass]
public class SidebarAnimationServiceTests
{
    private SidebarAnimationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new SidebarAnimationService();
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueForLastMessage()
    {
        // Arrange
        const int lastMessageIndex = 2;
        const int totalMessages = 3;

        // Act
        var result = _service.ShouldShowSidebar(lastMessageIndex, totalMessages);

        // Assert
        Assert.IsTrue(result, "Should return true when at last message index");
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForFirstMessage()
    {
        // Arrange
        const int firstMessageIndex = 0;
        const int totalMessages = 3;

        // Act
        var result = _service.ShouldShowSidebar(firstMessageIndex, totalMessages);

        // Assert
        Assert.IsFalse(result, "Should return false for first message");
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForMiddleMessage()
    {
        // Arrange
        const int middleMessageIndex = 1;
        const int totalMessages = 3;

        // Act
        var result = _service.ShouldShowSidebar(middleMessageIndex, totalMessages);

        // Assert
        Assert.IsFalse(result, "Should return false for middle message");
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueForSingleMessage()
    {
        // Arrange
        const int onlyMessageIndex = 0;
        const int totalMessages = 1;

        // Act
        var result = _service.ShouldShowSidebar(onlyMessageIndex, totalMessages);

        // Assert
        Assert.IsTrue(result, "Should return true when there is only one message");
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsFalseForZeroMessages()
    {
        // Arrange
        const int messageIndex = 0;
        const int totalMessages = 0;

        // Act
        var result = _service.ShouldShowSidebar(messageIndex, totalMessages);

        // Assert
        Assert.IsFalse(result, "Should return false when there are no messages");
    }

    [TestMethod]
    [DataRow(0, 5)]
    [DataRow(1, 5)]
    [DataRow(2, 5)]
    [DataRow(3, 5)]
    public void ShouldShowSidebar_ReturnsFalseForAllButLastIndex(int messageIndex, int totalMessages)
    {
        // Act
        var result = _service.ShouldShowSidebar(messageIndex, totalMessages);

        // Assert
        Assert.IsFalse(result, $"Should return false for index {messageIndex} when total is {totalMessages}");
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueOnlyForExactLastIndex()
    {
        // Arrange
        const int totalMessages = 10;

        // Act & Assert
        for (int i = 0; i < totalMessages; i++)
        {
            var result = _service.ShouldShowSidebar(i, totalMessages);
            if (i == totalMessages - 1)
            {
                Assert.IsTrue(result, $"Should return true at last index {totalMessages - 1}");
            }
            else
            {
                Assert.IsFalse(result, $"Should return false at index {i}");
            }
        }
    }

    [TestMethod]
    public void ShouldShowSidebar_ReturnsTrueWhenIndexEqualsLargeMessageCount()
    {
        // Arrange
        const int largeCount = 1000;
        const int lastIndex = largeCount - 1;

        // Act
        var result = _service.ShouldShowSidebar(lastIndex, largeCount);

        // Assert
        Assert.IsTrue(result, "Should correctly identify last message in large sequence");
    }
}
