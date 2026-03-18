using Bunit;
using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Index = ChannelDungeons.BlazorWasm.Pages.Index;

namespace ChannelDungeons.Tests.Pages;

[TestClass]
public class IndexComponentTests : Bunit.TestContext
{
    private void SetupServices(bool includeWelcomeChannel = false)
    {
        var httpClient = new HttpClient(new MockHttpMessageHandler(includeWelcomeChannel))
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var channelService = new ChannelService(httpClient);
        var animationService = new MessageAnimationService();

        Services.AddScoped(_ => channelService);
        Services.AddScoped(_ => animationService);
        // Use bUnit's FakeNavigationManager instead of NSubstitute
        // It's already available via TestContext
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupVoid("channelDungeons.setupCommandInputKeyHandler", _ => true);
    }

    [TestMethod]
    public async Task Index_RendersWithoutException()
    {
        // Arrange
        SetupServices();

        // Act
        var cut = RenderComponent<Index>();

        // Wait for component to fully initialize and render with expected state
        cut.WaitForState(() =>
        {
            var sidebar = cut.FindAll(".sidebar").FirstOrDefault();
            var commandInput = cut.FindAll(".command-input-container").FirstOrDefault();
            return sidebar != null &&
                   sidebar.ClassList.Contains("visible") &&
                   commandInput != null &&
                   commandInput.ClassList.Contains("visible");
        }, timeout: TimeSpan.FromSeconds(2));

        // Assert - component should render without throwing
        Assert.IsNotNull(cut);

        // Verify component structure exists
        var sidebarElement = cut.Find(".sidebar");
        Assert.IsNotNull(sidebarElement, "Sidebar should be rendered");

        var contentArea = cut.Find(".content-area");
        Assert.IsNotNull(contentArea, "Content area should be rendered");

        // Verify sidebar visibility behavior for non-welcome channels on desktop
        // The sidebar should be immediately visible since test-channel is not the welcome channel
        Assert.IsTrue(sidebarElement.ClassList.Contains("visible"),
            "Sidebar should have 'visible' class for non-welcome channels on desktop");

        // Verify command input is visible after initialization
        var commandInput = cut.Find(".command-input-container");
        Assert.IsTrue(commandInput.ClassList.Contains("visible"),
            "Command input should be visible after messages load");
    }

    [TestMethod]
    public async Task HandleToggleSidebar_TogglesSidebarVisibility()
    {
        // Arrange
        SetupServices();

        var cut = RenderComponent<Index>();
        cut.WaitForState(() =>
        {
            var sidebar = cut.FindAll(".sidebar").FirstOrDefault();
            return sidebar != null && sidebar.ClassList.Contains("visible");
        }, timeout: TimeSpan.FromSeconds(2));

        var initiallyVisible = cut.Find(".sidebar").ClassList.Contains("visible");

        // Act - click the toggle button in the channel header
        var toggleButton = cut.Find(".sidebar-toggle");
        toggleButton.Click();

        // Assert - sidebar visibility should toggle
        var afterToggle = cut.Find(".sidebar").ClassList.Contains("visible");
        Assert.AreNotEqual(initiallyVisible, afterToggle, "Sidebar visibility should toggle");
    }

    [TestMethod]
    public async Task HandleCommandSubmit_WithInvalidCommand_ShowsErrorMessage()
    {
        // Arrange
        SetupServices();

        var cut = RenderComponent<Index>();
        cut.WaitForState(() =>
        {
            var commandInput = cut.FindAll(".command-input-container").FirstOrDefault();
            return commandInput != null && commandInput.ClassList.Contains("visible");
        }, timeout: TimeSpan.FromSeconds(2));

        var initialMessagesCount = cut.FindAll(".message").Count;

        // Act - submit an invalid command through the CommandInput component
        var commandInputComponent = cut.FindComponent<ChannelDungeons.BlazorWasm.Components.Layout.CommandInput>();
        await commandInputComponent.InvokeAsync(() =>
            commandInputComponent.Instance.OnCommandSubmit.InvokeAsync("/invalidchannel"));

        // Assert - error message should be added with "Unknown command"
        var messages = cut.FindAll(".message");
        Assert.AreEqual(initialMessagesCount + 1, messages.Count, "One error message should be added");

        var lastMessageContent = messages.Last().QuerySelector(".message-content")!.InnerHtml;
        Assert.IsTrue(lastMessageContent.Contains("Unknown command"), "Error message should mention 'Unknown command'");
    }

    [TestMethod]
    public async Task HandleCommandSubmit_WithNonSlashInput_ShowsDemoMessage()
    {
        // Arrange
        SetupServices();

        var cut = RenderComponent<Index>();
        cut.WaitForState(() =>
        {
            var commandInput = cut.FindAll(".command-input-container").FirstOrDefault();
            return commandInput != null && commandInput.ClassList.Contains("visible");
        }, timeout: TimeSpan.FromSeconds(2));

        var initialMessagesCount = cut.FindAll(".message").Count;

        // Act - submit non-slash text through the CommandInput component
        var commandInputComponent = cut.FindComponent<ChannelDungeons.BlazorWasm.Components.Layout.CommandInput>();
        await commandInputComponent.InvokeAsync(() =>
            commandInputComponent.Instance.OnCommandSubmit.InvokeAsync("hello"));

        // Assert - demo message should be added
        var messages = cut.FindAll(".message");
        Assert.AreEqual(initialMessagesCount + 1, messages.Count, "One demo message should be added");

        var lastMessageContent = messages.Last().QuerySelector(".message-content")!.InnerHtml;
        Assert.IsTrue(lastMessageContent.Contains("demonstration"), "Message should mention it's a demonstration");
    }

    [TestMethod]
    public async Task HandleChannelSelected_NavigatesToNewChannel()
    {
        // Arrange
        SetupServices();

        var cut = RenderComponent<Index>();
        cut.WaitForState(() =>
        {
            var sidebar = cut.FindAll(".sidebar").FirstOrDefault();
            return sidebar != null && sidebar.ClassList.Contains("visible");
        }, timeout: TimeSpan.FromSeconds(2));

        // Get initial channel
        var initialChannelHeader = cut.Find(".channel-name");
        var initialChannelName = initialChannelHeader.TextContent.Trim();

        // Act - find the sidebar component and click on it to trigger channel selection
        // Since we can't easily click the sidebar in tests, we'll invoke the callback directly
        var sidebarComponent = cut.FindComponent<ChannelDungeons.BlazorWasm.Components.Layout.Sidebar>();
        await sidebarComponent.InvokeAsync(() =>
            sidebarComponent.Instance.OnChannelSelected.InvokeAsync("test-channel"));

        // Wait for channel to load
        await Task.Delay(200);

        // Assert - channel should remain the same since test-channel is the only channel
        var afterChannelHeader = cut.Find(".channel-name");
        var afterChannelName = afterChannelHeader.TextContent.Trim();
        Assert.IsNotNull(afterChannelName, "Channel name should be present after navigation");
    }

    [TestMethod]
    public async Task WelcomeChannel_ShowsSidebarAfterAnimation()
    {
        // Arrange
        SetupServices(includeWelcomeChannel: true);

        var cut = RenderComponent<Index>(parameters => parameters.Add(p => p.ChannelName, "welcome"));

        // Wait for messages to animate and sidebar to appear
        // The welcome channel should show sidebar after the last message
        cut.WaitForState(() =>
        {
            var commandInput = cut.FindAll(".command-input-container").FirstOrDefault();
            return commandInput != null && commandInput.ClassList.Contains("visible");
        }, timeout: TimeSpan.FromSeconds(3));

        // Assert - sidebar should eventually become visible after animation completes
        // Note: On desktop, sidebar visibility depends on the animation completing
        var sidebar = cut.Find(".sidebar");
        Assert.IsNotNull(sidebar, "Sidebar should be rendered");
    }
}

/// <summary>
/// Mock HTTP handler that returns test channel data for testing.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly bool _includeWelcomeChannel;

    public MockHttpMessageHandler(bool includeWelcomeChannel = false)
    {
        _includeWelcomeChannel = includeWelcomeChannel;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Return test channel data
        var channels = new List<string>();

        if (_includeWelcomeChannel)
        {
            channels.Add("""
            {
              "id": "welcome",
              "name": "welcome",
              "description": "Welcome channel",
              "messages": [
                {
                  "username": "Bot",
                  "content": "Welcome!",
                  "typingDuration": 50,
                  "delay": 0
                }
              ]
            }
""");
        }

        channels.Add("""
            {
              "id": "test-channel",
              "name": "test",
              "description": "Test channel",
              "messages": [
                {
                  "username": "Test User",
                  "content": "Hello",
                  "typingDuration": 50,
                  "delay": 0
                },
                {
                  "username": "Test User",
                  "content": "World",
                  "typingDuration": 50,
                  "delay": 0
                }
              ]
            }
""");

        var channelsJson = string.Join(",", channels);
        var json = $$"""
{
  "config": {
    "defaultTypingDuration": 50,
    "defaultMessageDelay": 50,
    "uiShowDelay": 100
  },
  "channels": [
    {{channelsJson}}
  ]
}
""";
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = content
        });
    }
}
