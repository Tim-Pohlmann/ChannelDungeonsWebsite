using Bunit;
using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Pages;
using ChannelDungeons.BlazorWasm.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace ChannelDungeons.Tests.Pages;

[TestClass]
public class IndexTests : Bunit.TestContext
{
    [TestMethod]
    public async Task Index_ShowsSidebarAfterLastMessageAnimates()
    {
        // Arrange
        var channelService = Substitute.For<ChannelService>();
        var animationService = Substitute.For<MessageAnimationService>();
        var navigationManager = Substitute.For<Microsoft.AspNetCore.Components.NavigationManager>();

        var testChannel = new Channel
        {
            Id = "test",
            Name = "Test Channel",
            Description = "A test channel",
            Messages = new List<ChannelMessage>
            {
                new() { Username = "Bot", Content = "Hello", TypingDuration = 0, Delay = 0 },
                new() { Username = "Bot", Content = "World", TypingDuration = 0, Delay = 0 }
            }
        };

        var testConfig = new AppConfig
        {
            DefaultTypingDuration = 0,
            DefaultMessageDelay = 0,
            UiShowDelay = 0
        };

        channelService
            .GetAllChannelsAsync()
            .Returns(new List<Channel> { testChannel });

        channelService
            .GetChannelAsync(Arg.Any<string>())
            .Returns(testChannel);

        channelService
            .GetConfigAsync()
            .Returns(testConfig);

        channelService
            .GetCachedMessages(Arg.Any<string>())
            .Returns((List<Message>?)null);

        // Simulate animation service behavior
        animationService
            .AnimateMessagesAsync(
                Arg.Any<List<Message>>(),
                Arg.Any<Func<int, Task>>(),
                Arg.Any<Func<bool, Task>>(),
                Arg.Any<AppConfig>(),
                Arg.Any<CancellationToken>())
            .Returns(async x =>
            {
                var messages = x.ArgAt<List<Message>>(0);
                var onMessageAdded = x.ArgAt<Func<int, Task>>(1);
                var onTyping = x.ArgAt<Func<bool, Task>>(2);

                // Simulate typing and message animation
                for (int i = 0; i < messages.Count; i++)
                {
                    await onTyping(true);
                    await onTyping(false);
                    await onMessageAdded(i);
                }
            });

        Services.AddScoped(_ => channelService);
        Services.AddScoped(_ => animationService);
        Services.AddScoped(_ => navigationManager);

        // Act
        var cut = RenderComponent<Index>();

        // Assert - sidebar should be visible after animation completes
        var sidebarElement = cut.Find(".sidebar");
        Assert.IsNotNull(sidebarElement);

        // The sidebar should have the 'visible' class after the last message
        var visibleClass = sidebarElement.ClassList.Contains("visible");
        Assert.IsTrue(visibleClass, "Sidebar should be visible after final message animates");
    }
}
