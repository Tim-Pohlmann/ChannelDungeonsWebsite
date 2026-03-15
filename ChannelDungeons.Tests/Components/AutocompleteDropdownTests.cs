using Bunit;
using ChannelDungeons.BlazorWasm.Components.Autocomplete;
using ChannelDungeons.BlazorWasm.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Tests.Components;

[TestClass]
public class AutocompleteDropdownTests : Bunit.TestContext
{
    private static List<CommandSuggestion> Commands(params (string name, string desc)[] items)
        => items.Select(x => new CommandSuggestion(x.name, x.desc)).ToList();

    [TestMethod]
    public void RendersNothing_WhenNotVisible()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, false)
            .Add(x => x.Commands, Commands(("/general", "General channel"))));

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public void RendersNothing_WhenCommandsIsEmpty()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, new List<CommandSuggestion>()));

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public void RendersAllCommands_WhenVisibleWithCommands()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, Commands(("/general", "General channel"), ("/rules", "Rules channel"))));

        var items = cut.FindAll(".autocomplete-item");
        Assert.AreEqual(2, items.Count);
        Assert.AreEqual("/general", items[0].QuerySelector(".command-name")!.TextContent.Trim());
        Assert.AreEqual("/rules", items[1].QuerySelector(".command-name")!.TextContent.Trim());
    }

    [TestMethod]
    public void RendersCommandDescriptions_WhenVisibleWithCommands()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, Commands(("/general", "General channel"))));

        var item = cut.Find(".autocomplete-item");
        Assert.AreEqual("General channel", item.QuerySelector(".command-description")!.TextContent.Trim());
    }

    [TestMethod]
    public void AppliesSelectedClass_ToItemAtSelectedIndex()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, Commands(("/general", "General channel"), ("/rules", "Rules channel")))
            .Add(x => x.SelectedIndex, 0));

        var items = cut.FindAll(".autocomplete-item");
        Assert.IsTrue(items[0].ClassList.Contains("selected"));
        Assert.IsFalse(items[1].ClassList.Contains("selected"));
    }

    [TestMethod]
    public void InvokesCallback_WhenItemClicked()
    {
        CommandSuggestion? selected = null;
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, Commands(("/general", "General channel"), ("/rules", "Rules channel")))
            .Add(x => x.OnCommandSelected, EventCallback.Factory.Create<CommandSuggestion>(this, cmd => selected = cmd)));

        cut.FindAll(".autocomplete-item")[1].Click();

        Assert.IsNotNull(selected);
        Assert.AreEqual("/rules", selected.Name);
    }
}
