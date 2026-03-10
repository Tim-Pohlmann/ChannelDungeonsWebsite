using System.Net;
using System.Text.Json;
using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace ChannelDungeons.Tests.Services;

[TestClass]
public class ChannelServiceTests
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
        var json = JsonSerializer.Serialize(data ?? CreateSampleData(), CamelCaseOptions);
        mockHttp.When("http://localhost/data/channels.json").Respond("application/json", json);
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost/");
        return new ChannelService(client);
    }

    [TestMethod]
    public async Task GetAllChannelsAsync_ReturnsChannelsFromJson()
    {
        var service = CreateService();

        var channels = await service.GetAllChannelsAsync();

        Assert.AreEqual(2, channels.Count);
        Assert.AreEqual("general", channels[0].Id);
        Assert.AreEqual("rules", channels[1].Id);
    }

    [TestMethod]
    public async Task GetAllChannelsAsync_CachesData_OnlyOneHttpCall()
    {
        var callCount = 0;
        var mockHttp = new MockHttpMessageHandler();
        var json = JsonSerializer.Serialize(CreateSampleData(), CamelCaseOptions);
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

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public async Task GetAllChannelsAsync_ThrowsHttpRequestException_OnHttpFailure()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/data/channels.json")
            .Respond(HttpStatusCode.NotFound);
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost/");
        var service = new ChannelService(client);

        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => service.GetAllChannelsAsync());
    }

    [TestMethod]
    public async Task GetChannelAsync_ReturnsMatchingChannel()
    {
        var service = CreateService();

        var channel = await service.GetChannelAsync("general");

        Assert.IsNotNull(channel);
        Assert.AreEqual("general", channel.Id);
        Assert.AreEqual("General channel", channel.Description);
    }

    [TestMethod]
    public async Task GetChannelAsync_IsCaseInsensitive()
    {
        var service = CreateService();

        var channel = await service.GetChannelAsync("GENERAL");

        Assert.IsNotNull(channel);
        Assert.AreEqual("general", channel.Id);
    }

    [TestMethod]
    public async Task GetChannelAsync_ReturnsNull_ForUnknownId()
    {
        var service = CreateService();

        var channel = await service.GetChannelAsync("nonexistent");

        Assert.IsNull(channel);
    }

    [TestMethod]
    public async Task GetConfigAsync_ReturnsConfigFromJson()
    {
        var service = CreateService();

        var config = await service.GetConfigAsync();

        Assert.AreEqual(500, config.DefaultTypingDuration);
        Assert.AreEqual(100, config.DefaultMessageDelay);
        Assert.AreEqual(300, config.UiShowDelay);
    }

    [TestMethod]
    public async Task GetConfigAsync_ReturnsDefaultConfig_WhenConfigIsDefault()
    {
        var data = new ChannelData
        {
            Channels = [new Channel { Id = "x", Name = "x", Description = "x" }]
            // Config is intentionally left as default
        };
        var service = CreateService(data);

        var config = await service.GetConfigAsync();

        Assert.IsNotNull(config);
        Assert.AreEqual(1000, config.DefaultTypingDuration);
        Assert.AreEqual(200, config.DefaultMessageDelay);
    }

    [TestMethod]
    public async Task GetChannelCommandsAsync_ReturnsCommandsWithSlashPrefix()
    {
        var service = CreateService();

        var commands = await service.GetChannelCommandsAsync();

        Assert.AreEqual(2, commands.Count);
        CollectionAssert.Contains(commands, "/general");
        CollectionAssert.Contains(commands, "/rules");
    }

    [TestMethod]
    public void IsCached_ReturnsFalse_Initially()
    {
        var service = CreateService();

        Assert.IsFalse(service.IsCached("general"));
    }

    [TestMethod]
    public void IsCached_ReturnsTrue_AfterCaching()
    {
        var service = CreateService();
        var messages = new List<Message> { new() { Content = "Hello" } };

        service.CacheMessages("general", messages);

        Assert.IsTrue(service.IsCached("general"));
    }

    [TestMethod]
    public void GetCachedMessages_ReturnsNull_WhenNotCached()
    {
        var service = CreateService();

        var result = service.GetCachedMessages("general");

        Assert.IsNull(result);
    }

    [TestMethod]
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

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Hello", result[0].Content);
    }

    [TestMethod]
    public void CacheMessages_OverwritesPreviousCache()
    {
        var service = CreateService();
        var firstMessages = new List<Message> { new() { Content = "First" } };
        var secondMessages = new List<Message> { new() { Content = "Second" } };

        service.CacheMessages("general", firstMessages);
        service.CacheMessages("general", secondMessages);
        var result = service.GetCachedMessages("general");

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Second", result[0].Content);
    }

    [TestMethod]
    public void ClearCache_RemovesAllCachedMessages()
    {
        var service = CreateService();
        service.CacheMessages("general", [new() { Content = "Hello" }]);
        service.CacheMessages("rules", [new() { Content = "Rules" }]);

        service.ClearCache();

        Assert.IsFalse(service.IsCached("general"));
        Assert.IsFalse(service.IsCached("rules"));
    }
}
