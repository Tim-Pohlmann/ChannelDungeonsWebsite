using Bunit;
using ChannelDungeons.BlazorWasm.Components.Autocomplete;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Tests.Components;

[TestClass]
public class AutocompleteDropdownTests : Bunit.TestContext
{
    [TestMethod]
    public void RendersNothing_WhenNotVisible()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, false)
            .Add(x => x.Commands, new List<string> { "/general" }));

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public void RendersNothing_WhenCommandsIsEmpty()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, new List<string>()));

        Assert.AreEqual(0, cut.FindAll(".autocomplete-dropdown").Count);
    }

    [TestMethod]
    public void RendersAllCommands_WhenVisibleWithCommands()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, new List<string> { "/general", "/rules" }));

        var items = cut.FindAll(".autocomplete-item");
        Assert.AreEqual(2, items.Count);
        Assert.AreEqual("/general", items[0].TextContent.Trim());
        Assert.AreEqual("/rules", items[1].TextContent.Trim());
    }

    [TestMethod]
    public void AppliesSelectedClass_ToItemAtSelectedIndex()
    {
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, new List<string> { "/general", "/rules" })
            .Add(x => x.SelectedIndex, 0));

        var items = cut.FindAll(".autocomplete-item");
        Assert.IsTrue(items[0].ClassList.Contains("selected"));
        Assert.IsFalse(items[1].ClassList.Contains("selected"));
    }

    [TestMethod]
    public void InvokesCallback_WhenItemClicked()
    {
        string? selected = null;
        var cut = RenderComponent<AutocompleteDropdown>(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Commands, new List<string> { "/general", "/rules" })
            .Add(x => x.OnCommandSelected, EventCallback.Factory.Create<string>(this, cmd => selected = cmd)));

        cut.FindAll(".autocomplete-item")[1].Click();

        Assert.AreEqual("/rules", selected);
    }
}
