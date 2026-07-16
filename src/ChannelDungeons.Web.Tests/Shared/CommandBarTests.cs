using Bunit;
using ChannelDungeons.Web.Models;
using ChannelDungeons.Web.Services;
using ChannelDungeons.Web.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Web.Tests.Shared;

[TestClass]
public class CommandBarTests : Bunit.TestContext
{
    private static readonly List<ChannelInfo> Channels = new()
    {
        new() { Name = "welcome", Description = "d1" },
        new() { Name = "about", Description = "d2" },
        new() { Name = "features", Description = "d3" },
    };

    public CommandBarTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./js/app.js");
        Services.AddScoped(_ => new BrowserInterop(JSInterop.JSRuntime));
    }

    [TestMethod]
    public void TypingSlash_ShowsMatchingCommands()
    {
        var cut = RenderComponent<CommandBar>(p => p.Add(c => c.Channels, Channels));

        cut.Find("input").Input("/a");

        var dropdown = cut.Find(".autocomplete-dropdown");
        Assert.IsTrue(dropdown.ClassList.Contains("visible"));
        Assert.AreEqual(1, cut.FindAll(".autocomplete-item").Count);
        Assert.IsTrue(cut.Find(".command-name").TextContent.Contains("about"));
    }

    [TestMethod]
    public void TypingNonCommandText_HidesDropdown()
    {
        var cut = RenderComponent<CommandBar>(p => p.Add(c => c.Channels, Channels));

        cut.Find("input").Input("hello");

        Assert.IsFalse(cut.Find(".autocomplete-dropdown").ClassList.Contains("visible"));
    }

    [TestMethod]
    public void TypingUnknownCommand_HidesDropdown()
    {
        var cut = RenderComponent<CommandBar>(p => p.Add(c => c.Channels, Channels));

        cut.Find("input").Input("/zzz");

        Assert.IsFalse(cut.Find(".autocomplete-dropdown").ClassList.Contains("visible"));
    }

    [TestMethod]
    public void EscapeKey_HidesDropdown()
    {
        var cut = RenderComponent<CommandBar>(p => p.Add(c => c.Channels, Channels));
        cut.Find("input").Input("/a");

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.IsFalse(cut.Find(".autocomplete-dropdown").ClassList.Contains("visible"));
    }

    [TestMethod]
    public void EnterKey_SubmitsTrimmedCommand()
    {
        string? submitted = null;
        var cut = RenderComponent<CommandBar>(p => p
            .Add(c => c.Channels, Channels)
            .Add(c => c.OnSubmit, cmd => submitted = cmd));

        cut.Find("input").Input("  hello  ");
        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.AreEqual("hello", submitted);
        Assert.AreEqual(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    [TestMethod]
    public void EnterKey_WhenDropdownVisible_SelectsMatchInsteadOfSubmitting()
    {
        string? submitted = null;
        var cut = RenderComponent<CommandBar>(p => p
            .Add(c => c.Channels, Channels)
            .Add(c => c.OnSubmit, cmd => submitted = cmd));

        cut.Find("input").Input("/about");
        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.IsNull(submitted);
        Assert.AreEqual("/about", cut.Find("input").GetAttribute("value"));
        Assert.IsFalse(cut.Find(".autocomplete-dropdown").ClassList.Contains("visible"));

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.AreEqual("/about", submitted);
    }

    [TestMethod]
    public void ArrowDownThenEnter_SelectsSecondMatch()
    {
        var cut = RenderComponent<CommandBar>(p => p.Add(c => c.Channels, Channels));

        cut.Find("input").Input("/");
        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.AreEqual("/about", cut.Find("input").GetAttribute("value"));
    }

    [TestMethod]
    public void ClickingAutocompleteItem_FillsInputWithCommand()
    {
        var cut = RenderComponent<CommandBar>(p => p.Add(c => c.Channels, Channels));
        cut.Find("input").Input("/f");

        cut.Find(".autocomplete-item").MouseDown();

        Assert.AreEqual("/features", cut.Find("input").GetAttribute("value"));
    }
}
