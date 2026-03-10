using System.Net;
using System.Text.Json;
using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
using RichardSzalay.MockHttp;

namespace ChannelDungeons.Tests.Services;

public class ChannelServiceTests
{
    private static ChannelData CreateSampleData() => new()
    {
        Config = new AppConfig
        {
            DefaultTypingDuration = 500,
            DefaultMessageDelay = 100,
            UiShowDelay = 300
        },
        Channels =
        [
            new Channel { Id = "general", Name = "general", Description = "General channel" },
            new Channel { Id = "rules", Name = "rules", Description = "Rules channel" }
        ]
    };

    private static ChannelService CreateService(ChannelData? data = null)
    {
        var mockHttp = new MockHttpMessageHandler();
        var json = JsonSerializer.Serialize(data ?? CreateSampleData(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        mockHttp.When("http://localhost/data/channels.json").Respond("application/json", json);
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost/");
        return new ChannelService(client);
    }

    [Fact]
    public async Task GetAllChannelsAsync_ReturnsChannelsFromJson()
    {
        var service = CreateService();

        var channels = await service.GetAllChannelsAsync();

        Assert.Equal(2, channels.Count);
        Assert.Equal("general", channels[0].Id);
        Assert.Equal("rules", channels[1].Id);
    }

    [Fact]
    public async Task GetAllChannelsAsync_CachesData_OnlyOneHttpCall()
    {
        var callCount = 0;
        var mockHttp = new MockHttpMessageHandler();
        var json = JsonSerializer.Serialize(CreateSampleData(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        mockHttp.When("http://localhost/data/channels.json")
            .Respond(_ =>
            {
                callCount++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost/");
        var service = new ChannelService(client);

        await service.GetAllChannelsAsync();
        await service.GetAllChannelsAsync();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetAllChannelsAsync_ThrowsHttpRequestException_OnHttpFailure()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/data/channels.json")
            .Respond(HttpStatusCode.NotFound);
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost/");
        var service = new ChannelService(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetAllChannelsAsync());
    }

    [Fact]
    public async Task GetChannelAsync_ReturnsMatchingChannel()
    {
        var service = CreateService();

        var channel = await service.GetChannelAsync("general");

        Assert.NotNull(channel);
        Assert.Equal("general", channel.Id);
        Assert.Equal("General channel", channel.Description);
    }

    [Fact]
    public async Task GetChannelAsync_IsCaseInsensitive()
    {
        var service = CreateService();

        var channel = await service.GetChannelAsync("GENERAL");

        Assert.NotNull(channel);
        Assert.Equal("general", channel.Id);
    }

    [Fact]
    public async Task GetChannelAsync_ReturnsNull_ForUnknownId()
    {
        var service = CreateService();

        var channel = await service.GetChannelAsync("nonexistent");

        Assert.Null(channel);
    }

    [Fact]
    public async Task GetConfigAsync_ReturnsConfigFromJson()
    {
        var service = CreateService();

        var config = await service.GetConfigAsync();

        Assert.Equal(500, config.DefaultTypingDuration);
        Assert.Equal(100, config.DefaultMessageDelay);
        Assert.Equal(300, config.UiShowDelay);
    }

    [Fact]
    public async Task GetConfigAsync_ReturnsDefaultConfig_WhenConfigIsNull()
    {
        var data = new ChannelData
        {
            Channels = [new Channel { Id = "x", Name = "x", Description = "x" }]
            // Config is intentionally left as default
        };
        var service = CreateService(data);

        var config = await service.GetConfigAsync();

        Assert.NotNull(config);
        Assert.Equal(1000, config.DefaultTypingDuration);
        Assert.Equal(200, config.DefaultMessageDelay);
    }

    [Fact]
    public async Task GetChannelCommandsAsync_ReturnsCommandsWithSlashPrefix()
    {
        var service = CreateService();

        var commands = await service.GetChannelCommandsAsync();

        Assert.Equal(2, commands.Count);
        Assert.Contains("/general", commands);
        Assert.Contains("/rules", commands);
    }

    [Fact]
    public void IsCached_ReturnsFalse_Initially()
    {
        var service = CreateService();

        Assert.False(service.IsCached("general"));
    }

    [Fact]
    public void IsCached_ReturnsTrue_AfterCaching()
    {
        var service = CreateService();
        var messages = new List<Message> { new() { Content = "Hello" } };

        service.CacheMessages("general", messages);

        Assert.True(service.IsCached("general"));
    }

    [Fact]
    public void GetCachedMessages_ReturnsNull_WhenNotCached()
    {
        var service = CreateService();

        var result = service.GetCachedMessages("general");

        Assert.Null(result);
    }

    [Fact]
    public void GetCachedMessages_ReturnsMessages_AfterCaching()
    {
        var service = CreateService();
        var messages = new List<Message>
        {
            new() { Content = "Hello", Username = "Bot" },
            new() { Content = "World", Username = "Bot" }
        };

        service.CacheMessages("general", messages);
        var result = service.GetCachedMessages("general");

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Hello", result[0].Content);
    }

    [Fact]
    public void CacheMessages_OverwritesPreviousCache()
    {
        var service = CreateService();
        var firstMessages = new List<Message> { new() { Content = "First" } };
        var secondMessages = new List<Message> { new() { Content = "Second" } };

        service.CacheMessages("general", firstMessages);
        service.CacheMessages("general", secondMessages);
        var result = service.GetCachedMessages("general");

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Second", result[0].Content);
    }

    [Fact]
    public void ClearCache_RemovesAllCachedMessages()
    {
        var service = CreateService();
        service.CacheMessages("general", [new() { Content = "Hello" }]);
        service.CacheMessages("rules", [new() { Content = "Rules" }]);

        service.ClearCache();

        Assert.False(service.IsCached("general"));
        Assert.False(service.IsCached("rules"));
    }
}
