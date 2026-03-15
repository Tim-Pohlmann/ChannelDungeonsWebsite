using Bunit;
using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
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
        var httpClient = new HttpClient(new MockHttpMessageHandler());
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

        // Wait for channel loading and message animation to complete
        await Task.Delay(600);

        // Assert - component should render without throwing
        Assert.IsNotNull(cut);

        // Verify component structure exists
        var sidebarElement = cut.Find(".sidebar");
        Assert.IsNotNull(sidebarElement, "Sidebar should be rendered");

        var contentArea = cut.Find(".content-area");
        Assert.IsNotNull(contentArea, "Content area should be rendered");
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
  "channels": [
    {
      "id": "test-channel",
      "name": "test",
      "description": "Test channel",
      "messages": [
        {
          "username": "Test User",
          "content": "Hello",
          "typingDuration": 100,
          "delay": 0
        },
        {
          "username": "Test User",
          "content": "World",
          "typingDuration": 100,
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
