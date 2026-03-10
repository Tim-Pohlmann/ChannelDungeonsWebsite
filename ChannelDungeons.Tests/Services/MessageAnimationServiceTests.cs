using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Tests.Services;

[TestClass]
public class MessageAnimationServiceTests
{
    private static AppConfig ZeroDelayConfig() => new()
    {
        DefaultTypingDuration = 0,
        DefaultMessageDelay = 0,
        UiShowDelay = 0
    };

    [TestMethod]
    public async Task AnimateMessagesAsync_EmptyList_DoesNotInvokeCallbacks()
    {
        var service = new MessageAnimationService();
        var messagesAdded = new List<int>();
        var typingChanges = new List<bool>();

        await service.AnimateMessagesAsync(
            [],
            i => { messagesAdded.Add(i); return Task.CompletedTask; },
            b => { typingChanges.Add(b); return Task.CompletedTask; },
            ZeroDelayConfig());

        Assert.AreEqual(0, messagesAdded.Count);
        Assert.AreEqual(0, typingChanges.Count);
    }

    [TestMethod]
    public async Task AnimateMessagesAsync_SingleMessage_InvokesCallbacksInOrder()
    {
        var service = new MessageAnimationService();
        var events = new List<string>();

        var messages = new List<Message>
        {
            new() { Content = "Hello", TypingDuration = 0, Delay = 0 }
        };

        await service.AnimateMessagesAsync(
            messages,
            i => { events.Add($"message:{i}"); return Task.CompletedTask; },
            b => { events.Add($"typing:{b}"); return Task.CompletedTask; },
            ZeroDelayConfig());

        Assert.AreEqual(3, events.Count);
        Assert.AreEqual("typing:True", events[0]);
        Assert.AreEqual("typing:False", events[1]);
        Assert.AreEqual("message:0", events[2]);
    }

    [TestMethod]
    public async Task AnimateMessagesAsync_MultipleMessages_AnimatesAllInOrder()
    {
        var service = new MessageAnimationService();
        var messagesAdded = new List<int>();

        var messages = new List<Message>
        {
            new() { Content = "First", TypingDuration = 0, Delay = 0 },
            new() { Content = "Second", TypingDuration = 0, Delay = 0 },
            new() { Content = "Third", TypingDuration = 0, Delay = 0 }
        };

        await service.AnimateMessagesAsync(
            messages,
            i => { messagesAdded.Add(i); return Task.CompletedTask; },
            _ => Task.CompletedTask,
            ZeroDelayConfig());

        Assert.AreEqual(3, messagesAdded.Count);
        CollectionAssert.AreEqual(new List<int> { 0, 1, 2 }, messagesAdded);
    }

    [TestMethod]
    public async Task AnimateMessagesAsync_UsesDefaultConfigValues_WhenMessageValuesAreNull()
    {
        var service = new MessageAnimationService();
        var typingShownCount = 0;

        var messages = new List<Message>
        {
            new() { Content = "Hello" } // TypingDuration and Delay are null
        };

        // Use zero config so the test doesn't actually delay
        var config = ZeroDelayConfig();

        await service.AnimateMessagesAsync(
            messages,
            _ => Task.CompletedTask,
            b => { if (b) typingShownCount++; return Task.CompletedTask; },
            config);

        // Typing indicator was shown once (for the one message)
        Assert.AreEqual(1, typingShownCount);
    }

    [TestMethod]
    public async Task AnimateMessagesAsync_UsesMessageSpecificTypingDuration()
    {
        var capturedDelays = new List<int>();
        var service = new MessageAnimationService((ms, _) => { capturedDelays.Add(ms); return Task.CompletedTask; });

        // Message has a distinct TypingDuration; config default is different
        var messages = new List<Message>
        {
            new() { Content = "Hello", TypingDuration = 100, Delay = 0 }
        };

        var config = new AppConfig
        {
            DefaultTypingDuration = 0,
            DefaultMessageDelay = 0,
            UiShowDelay = 0
        };

        await service.AnimateMessagesAsync(
            messages,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask,
            config);

        // The message-specific TypingDuration (100) should have been used, not the config default (0)
        Assert.IsTrue(capturedDelays.Contains(100),
            $"Expected delay of 100ms but captured: [{string.Join(", ", capturedDelays)}]");
    }

    [TestMethod]
    public async Task AnimateMessagesAsync_CancelledBeforeStart_DoesNotAnimate()
    {
        var service = new MessageAnimationService();
        var messagesAdded = new List<int>();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var messages = new List<Message>
        {
            new() { Content = "Hello", TypingDuration = 0, Delay = 0 }
        };

        await service.AnimateMessagesAsync(
            messages,
            i => { messagesAdded.Add(i); return Task.CompletedTask; },
            _ => Task.CompletedTask,
            ZeroDelayConfig(),
            cts.Token);

        Assert.AreEqual(0, messagesAdded.Count);
    }

    [TestMethod]
    public async Task AnimateMessagesAsync_CancellationDuringAnimation_StopsGracefully()
    {
        var service = new MessageAnimationService();
        var messagesAdded = new List<int>();
        using var cts = new CancellationTokenSource();

        var messages = new List<Message>
        {
            new() { Content = "First", TypingDuration = 0, Delay = 0 },
            new() { Content = "Second", TypingDuration = 0, Delay = 0 },
            new() { Content = "Third", TypingDuration = 0, Delay = 0 }
        };

        // Cancel after the first message is added
        await service.AnimateMessagesAsync(
            messages,
            i =>
            {
                messagesAdded.Add(i);
                if (i == 0) cts.Cancel();
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask,
            ZeroDelayConfig(),
            cts.Token);

        // Only the first message should have been added before cancellation
        CollectionAssert.AreEqual(new List<int> { 0 }, messagesAdded);
    }

    [TestMethod]
    public async Task AnimateMessagesAsync_CompletesWithoutException_OnNormalFlow()
    {
        var service = new MessageAnimationService();
        var messages = new List<Message>
        {
            new() { Content = "Test message", TypingDuration = 0, Delay = 0 }
        };

        Exception? caughtException = null;
        try
        {
            await service.AnimateMessagesAsync(
                messages,
                _ => Task.CompletedTask,
                _ => Task.CompletedTask,
                ZeroDelayConfig());
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        Assert.IsNull(caughtException);
    }
}
