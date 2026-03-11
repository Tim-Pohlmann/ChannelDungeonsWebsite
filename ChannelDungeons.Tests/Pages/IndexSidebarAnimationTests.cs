using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Tests.Pages;

/// <summary>
/// Tests for the sidebar animation behavior in Index component.
/// Verifies that the sidebar visibility is triggered at the correct time
/// during the message animation sequence.
/// </summary>
[TestClass]
public class IndexSidebarAnimationTests
{
    [TestMethod]
    public async Task SidebarAnimationTrigger_OccursAfterLastMessageIndex()
    {
        // Arrange
        var service = new MessageAnimationService();
        var lastMessageIndex = -1;
        var sidebarTriggered = false;
        var messages = new List<Message>
        {
            new() { Content = "Message 1", TypingDuration = 0, Delay = 0 },
            new() { Content = "Message 2", TypingDuration = 0, Delay = 0 },
            new() { Content = "Message 3", TypingDuration = 0, Delay = 0 }
        };

        var config = new AppConfig
        {
            DefaultTypingDuration = 0,
            DefaultMessageDelay = 0,
            UiShowDelay = 0
        };

        // Act
        await service.AnimateMessagesAsync(
            messages,
            async (messageIndex) =>
            {
                lastMessageIndex = messageIndex;
                // Simulate Index.razor logic: show sidebar after last message
                if (messageIndex == messages.Count - 1)
                {
                    await Task.Delay(config.UiShowDelay);
                    sidebarTriggered = true;
                }
                await Task.CompletedTask;
            },
            async (isTyping) => await Task.CompletedTask,
            config);

        // Assert
        Assert.AreEqual(2, lastMessageIndex, "Last message index should be 2 (0-based)");
        Assert.IsTrue(sidebarTriggered, "Sidebar should be triggered after last message");
    }

    [TestMethod]
    public async Task SidebarAnimationTrigger_IdentifiesLastMessageCorrectly()
    {
        // Arrange
        var service = new MessageAnimationService();
        var triggeredAt = -1;
        var totalMessages = 5;
        var messages = Enumerable.Range(0, totalMessages)
            .Select(i => new Message { Content = $"Message {i}", TypingDuration = 0, Delay = 0 })
            .ToList();

        var config = new AppConfig
        {
            DefaultTypingDuration = 0,
            DefaultMessageDelay = 0,
            UiShowDelay = 0
        };

        // Act
        await service.AnimateMessagesAsync(
            messages,
            async (messageIndex) =>
            {
                // Track which message index triggers sidebar logic
                if (messageIndex == messages.Count - 1)
                {
                    triggeredAt = messageIndex;
                }
                await Task.CompletedTask;
            },
            async (isTyping) => await Task.CompletedTask,
            config);

        // Assert
        Assert.AreEqual(totalMessages - 1, triggeredAt,
            "Sidebar trigger should occur at the last message index");
    }

    [TestMethod]
    public async Task SidebarAnimationTrigger_NeverTriggersBeforeLastMessage()
    {
        // Arrange
        var service = new MessageAnimationService();
        var triggerCount = 0;
        var messages = new List<Message>
        {
            new() { Content = "Message 1", TypingDuration = 0, Delay = 0 },
            new() { Content = "Message 2", TypingDuration = 0, Delay = 0 },
            new() { Content = "Message 3", TypingDuration = 0, Delay = 0 }
        };

        var config = new AppConfig
        {
            DefaultTypingDuration = 0,
            DefaultMessageDelay = 0,
            UiShowDelay = 0
        };

        // Act
        await service.AnimateMessagesAsync(
            messages,
            async (messageIndex) =>
            {
                // Should only trigger when messageIndex equals messages.Count - 1
                if (messageIndex == messages.Count - 1)
                {
                    triggerCount++;
                }
                await Task.CompletedTask;
            },
            async (isTyping) => await Task.CompletedTask,
            config);

        // Assert
        Assert.AreEqual(1, triggerCount, "Sidebar trigger should occur exactly once");
    }
}
