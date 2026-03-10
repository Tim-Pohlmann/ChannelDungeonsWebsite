using System.Text.Json;
using Bunit;
using ChannelDungeons.BlazorWasm.Components.Layout;
using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace ChannelDungeons.Tests.Components;

[TestClass]
public class CommandInputTests : Bunit.TestContext
{
    [TestInitialize]
    public void Setup()
    {
        var data = new ChannelData
        {
            Config = new AppConfig(),
            Channels =
            [
                new Channel { Id = "general", Name = "general", Description = "General" },
                new Channel { Id = "rules", Name = "rules", Description = "Rules" }
            ]
        };
        var mockHttp = new MockHttpMessageHandler();
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        mockHttp.When("http://localhost/data/channels.json").Respond("application/json", json);
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost/");
        Services.AddScoped(_ => new ChannelService(client));
    }

    [TestMethod]
    public void AutocompleteHidden_Initially()
    {
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false));

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public async Task AutocompleteShown_WhenInputMatchesCommand()
    {
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/gen" });

        Assert.AreEqual(1, cut.FindAll(".autocomplete-dropdown").Count);
        Assert.AreEqual(1, cut.FindAll(".autocomplete-item").Count);
        Assert.AreEqual("/general", cut.Find(".autocomplete-item").TextContent.Trim());
    }

    [TestMethod]
    public async Task AutocompleteHidden_WhenInputHasNoSlash()
    {
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "general" });

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public async Task AutocompleteHidden_WhenInputIsOnlySlash()
    {
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/" });

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public async Task FilterIsCaseInsensitive()
    {
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/GEN" });

        Assert.AreEqual("/general", cut.Find(".autocomplete-item").TextContent.Trim());
    }

    [TestMethod]
    public async Task AllCommandsShown_WhenInputMatchesAll()
    {
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/" + "x" }); // no matches

        Assert.AreEqual(0, cut.FindAll(".autocomplete-item").Count);
    }
}
