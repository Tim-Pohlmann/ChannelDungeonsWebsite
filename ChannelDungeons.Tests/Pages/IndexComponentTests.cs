using Bunit;
using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Index = ChannelDungeons.BlazorWasm.Pages.Index;

namespace ChannelDungeons.Tests.Pages;

[TestClass]
public class IndexComponentTests : Bunit.TestContext
{
    [TestMethod]
    public async Task Index_RendersWithoutException()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler())
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var channelService = new ChannelService(httpClient);
        var animationService = new MessageAnimationService();
        var navigationManager = Substitute.For<Microsoft.AspNetCore.Components.NavigationManager>();

        Services.AddScoped(_ => channelService);
        Services.AddScoped(_ => animationService);
        Services.AddScoped(_ => navigationManager);

        // Configure JSRuntime: use loose mode so unmocked calls return defaults (false for bool = desktop)
        JSInterop.Mode = JSRuntimeMode.Loose;

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
        var httpClient = new HttpClient(new MockHttpMessageHandler())
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var channelService = new ChannelService(httpClient);
        var animationService = new MessageAnimationService();
        var navigationManager = Substitute.For<Microsoft.AspNetCore.Components.NavigationManager>();

        Services.AddScoped(_ => channelService);
        Services.AddScoped(_ => animationService);
        Services.AddScoped(_ => navigationManager);
        JSInterop.Mode = JSRuntimeMode.Loose;

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
        var httpClient = new HttpClient(new MockHttpMessageHandler())
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var channelService = new ChannelService(httpClient);
        var animationService = new MessageAnimationService();
        var navigationManager = Substitute.For<Microsoft.AspNetCore.Components.NavigationManager>();

        Services.AddScoped(_ => channelService);
        Services.AddScoped(_ => animationService);
        Services.AddScoped(_ => navigationManager);
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupVoid("channelDungeons.setupCommandInputKeyHandler", _ => true);

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
        var httpClient = new HttpClient(new MockHttpMessageHandler())
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var channelService = new ChannelService(httpClient);
        var animationService = new MessageAnimationService();
        var navigationManager = Substitute.For<Microsoft.AspNetCore.Components.NavigationManager>();

        Services.AddScoped(_ => channelService);
        Services.AddScoped(_ => animationService);
        Services.AddScoped(_ => navigationManager);
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupVoid("channelDungeons.setupCommandInputKeyHandler", _ => true);

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
}

/// <summary>
/// Mock HTTP handler that returns test channel data for testing.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Return test channel data with a non-welcome channel so sidebar is visible immediately
        var json = """
{
  "config": {
    "defaultTypingDuration": 50,
    "defaultMessageDelay": 50,
    "uiShowDelay": 100
  },
  "channels": [
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
