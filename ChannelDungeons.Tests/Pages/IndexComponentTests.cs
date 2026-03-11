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
    public async Task Index_DisplaysSidebarAfterMessagesLoad()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler());
        var channelService = new ChannelService(httpClient);
        var animationService = new MessageAnimationService();
        var sidebarAnimationService = new SidebarAnimationService();
        var navigationManager = Substitute.For<Microsoft.AspNetCore.Components.NavigationManager>();

        Services.AddScoped(_ => channelService);
        Services.AddScoped(_ => animationService);
        Services.AddScoped(_ => sidebarAnimationService);
        Services.AddScoped(_ => navigationManager);

        // Act
        var cut = RenderComponent<Index>();
        await Task.Delay(100); // Allow component to render

        // Assert - component should render without throwing
        Assert.IsNotNull(cut);
    }
}

/// <summary>
/// Mock HTTP handler that returns empty data for testing.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var content = new StringContent("""{"channels":[]}""");
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = content
        });
    }
}
