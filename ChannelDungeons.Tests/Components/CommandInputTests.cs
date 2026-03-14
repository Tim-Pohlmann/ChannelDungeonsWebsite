using System.Text.Json;
using Bunit;
using ChannelDungeons.BlazorWasm.Components.Layout;
using ChannelDungeons.BlazorWasm.Models;
using ChannelDungeons.BlazorWasm.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace ChannelDungeons.Tests.Components;

[TestClass]
public class CommandInputTests : Bunit.TestContext
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private void RegisterService(IEnumerable<string> channelIds)
    {
        var data = new ChannelData
        {
            Config = new AppConfig(),
            Channels = channelIds
                .Select(id => new Channel { Id = id, Name = id, Description = $"{id} description" })
                .ToList()
        };
        var mockHttp = new MockHttpMessageHandler();
        var json = JsonSerializer.Serialize(data, CamelCaseOptions);
        mockHttp.When("http://localhost/data/channels.json").Respond("application/json", json);
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost/");
        Services.AddScoped(_ => new ChannelService(client));
    }

    // --- Filtering ---

    [TestMethod]
    public void AutocompleteHidden_Initially()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public async Task AutocompleteShown_WhenInputMatchesCommand()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/gen" });

        Assert.AreEqual("/general", cut.Find(".autocomplete-item .command-name").TextContent.Trim());
    }

    [TestMethod]
    public async Task AutocompleteShown_WhenInputIsOnlySlash()
    {
        // Typing just "/" should show all available commands
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/" });

        Assert.AreEqual(2, cut.FindAll(".autocomplete-item").Count);
    }

    [TestMethod]
    public async Task AutocompleteHidden_WhenInputHasNoSlash()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "general" });

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public async Task FilterIsCaseInsensitive()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/GEN" });

        Assert.AreEqual("/general", cut.Find(".autocomplete-item .command-name").TextContent.Trim());
    }

    [TestMethod]
    public async Task NoItemsShown_WhenInputMatchesNothing()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/xyz" });

        Assert.AreEqual(0, cut.FindAll(".autocomplete-item").Count);
    }

    [TestMethod]
    public async Task CommandDescriptions_ShownInAutocomplete()
    {
        RegisterService(["general"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/gen" });

        Assert.AreEqual("general description", cut.Find(".autocomplete-item .command-description").TextContent.Trim());
    }

    // --- Keyboard navigation ---

    [TestMethod]
    public async Task ArrowDown_SelectsFirstItem()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));
        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/gen" });

        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "ArrowDown" });

        Assert.IsTrue(cut.Find(".autocomplete-item").ClassList.Contains("selected"));
    }

    [TestMethod]
    public async Task ArrowDown_ClampsAtLastItem()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));
        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/r" });

        // Only one item (/rules) — pressing down twice should stay at index 0
        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "ArrowDown" });
        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "ArrowDown" });

        Assert.IsTrue(cut.Find(".autocomplete-item").ClassList.Contains("selected"));
    }

    [TestMethod]
    public async Task ArrowUp_RemovesSelection_WhenFirstItemIsSelected()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));
        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/gen" });
        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "ArrowDown" });

        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "ArrowUp" });

        Assert.IsFalse(cut.Find(".autocomplete-item").ClassList.Contains("selected"));
    }

    [TestMethod]
    public async Task Escape_HidesAutocomplete()
    {
        RegisterService(["general", "rules"]);
        var cut = RenderComponent<CommandInput>(p => p.Add(x => x.IsVisible, false));
        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/gen" });

        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "Escape" });

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    // --- Submission ---

    [TestMethod]
    public async Task Enter_SubmitsSelectedCommand_WhenItemSelected()
    {
        RegisterService(["general", "rules"]);
        string? submitted = null;
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false)
            .Add(x => x.OnCommandSubmit,
                EventCallback.Factory.Create<string>(this, cmd => submitted = cmd)));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/gen" });
        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "ArrowDown" });
        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "Enter" });

        Assert.AreEqual("/general", submitted);
    }

    [TestMethod]
    public async Task Enter_SubmitsSearchTerm_WhenNoAutocompleteMatch()
    {
        RegisterService(["general", "rules"]);
        string? submitted = null;
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false)
            .Add(x => x.OnCommandSubmit,
                EventCallback.Factory.Create<string>(this, cmd => submitted = cmd)));

        // /about has no match → autocomplete list is empty → direct submit
        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/about" });
        await cut.Find(".command-input").TriggerEventAsync("onkeydown",
            new KeyboardEventArgs { Key = "Enter" });

        Assert.AreEqual("/about", submitted);
    }

    [TestMethod]
    public async Task AutocompleteItemClick_SubmitsCommand()
    {
        RegisterService(["general", "rules"]);
        string? submitted = null;
        var cut = RenderComponent<CommandInput>(p => p
            .Add(x => x.IsVisible, false)
            .Add(x => x.OnCommandSubmit,
                EventCallback.Factory.Create<string>(this, cmd => submitted = cmd)));

        await cut.Find(".command-input").TriggerEventAsync("oninput",
            new ChangeEventArgs { Value = "/r" });
        cut.Find(".autocomplete-item").Click();

        Assert.AreEqual("/rules", submitted);
    }
}
