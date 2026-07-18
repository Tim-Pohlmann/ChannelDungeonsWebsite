using Bunit;
using ChannelDungeons.Web.Components;
using ChannelDungeons.Web.Models;
using Microsoft.AspNetCore.Components.Web;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class CommandInputTests : AppTestContext
{
    private static readonly IReadOnlyList<CommandSuggestion> Commands =
    [
        new("welcome", "Welcome!"),
        new("about", "About"),
        new("features", "Features")
    ];

    private IRenderedComponent<CommandInput> RenderInput(Action<string>? onSubmit = null) =>
        Render<CommandInput>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Commands, Commands)
            .Add(p => p.OnSubmit, input => onSubmit?.Invoke(input)));

    [TestMethod]
    public void TypingSlash_ShowsAllSuggestionsWithFirstSelected()
    {
        var cut = RenderInput();

        cut.Find("#command-input").Input("/");

        Assert.Contains("visible", cut.Find(".autocomplete-dropdown").ClassList);
        var items = cut.FindAll(".autocomplete-item");
        Assert.HasCount(3, items);
        Assert.Contains("selected", items[0].ClassList);
    }

    [TestMethod]
    public void TypingPrefix_FiltersSuggestions()
    {
        var cut = RenderInput();

        cut.Find("#command-input").Input("/a");

        var items = cut.FindAll(".autocomplete-item");
        Assert.HasCount(1, items);
        Assert.Contains("/about", items[0].TextContent);
    }

    [TestMethod]
    public void TextWithoutSlash_ShowsNoDropdown()
    {
        var cut = RenderInput();

        cut.Find("#command-input").Input("about");

        Assert.DoesNotContain("visible", cut.Find(".autocomplete-dropdown").ClassList);
    }

    [TestMethod]
    public void ArrowKeys_MoveSelectionAndClampAtEnds()
    {
        var cut = RenderInput();
        var input = cut.Find("#command-input");
        input.Input("/");

        input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Contains("selected", cut.FindAll(".autocomplete-item")[1].ClassList);

        input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Contains("selected", cut.FindAll(".autocomplete-item")[2].ClassList);

        input.KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Contains("selected", cut.FindAll(".autocomplete-item")[1].ClassList);
    }

    [TestMethod]
    public void Escape_HidesDropdown()
    {
        var cut = RenderInput();
        var input = cut.Find("#command-input");
        input.Input("/");

        input.KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.DoesNotContain("visible", cut.Find(".autocomplete-dropdown").ClassList);
    }

    [TestMethod]
    public void Tab_AcceptsSelectedSuggestion()
    {
        var cut = RenderInput();
        var input = cut.Find("#command-input");
        input.Input("/a");

        input.KeyDown(new KeyboardEventArgs { Key = "Tab" });

        Assert.AreEqual("/about", cut.Find("#command-input").GetAttribute("value"));
        Assert.DoesNotContain("visible", cut.Find(".autocomplete-dropdown").ClassList);
    }

    [TestMethod]
    public void MouseDownOnSuggestion_AcceptsIt()
    {
        var cut = RenderInput();
        cut.Find("#command-input").Input("/f");

        cut.Find(".autocomplete-item").MouseDown();

        Assert.AreEqual("/features", cut.Find("#command-input").GetAttribute("value"));
    }

    [TestMethod]
    public void Enter_SubmitsAndClearsInput()
    {
        string? submitted = null;
        var cut = RenderInput(input => submitted = input);
        var input = cut.Find("#command-input");
        input.Input("hello");

        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.AreEqual("hello", submitted);
        Assert.AreEqual(string.Empty, cut.Find("#command-input").GetAttribute("value"));
    }

    [TestMethod]
    public void EnterOnWhitespace_DoesNotSubmit()
    {
        var submitted = false;
        var cut = RenderInput(_ => submitted = true);
        var input = cut.Find("#command-input");
        input.Input("   ");

        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.IsFalse(submitted);
    }
}
