using ChannelDungeons.Web.Models;
using ChannelDungeons.Web.Services;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class AutocompleteFilterTests
{
    private static readonly IReadOnlyList<CommandSuggestion> Commands =
    [
        new("welcome", "Welcome!"),
        new("about", "About"),
        new("features", "Features")
    ];

    [TestMethod]
    public void InputWithoutSlash_ReturnsNothing()
    {
        Assert.IsEmpty(AutocompleteFilter.Filter(Commands, "about"));
    }

    [TestMethod]
    public void SlashOnly_ReturnsAllCommands()
    {
        Assert.HasCount(3, AutocompleteFilter.Filter(Commands, "/"));
    }

    [TestMethod]
    public void Prefix_ReturnsMatchingCommands()
    {
        var matches = AutocompleteFilter.Filter(Commands, "/a");

        Assert.HasCount(1, matches);
        Assert.AreEqual("about", matches[0].Name);
    }

    [TestMethod]
    public void Prefix_IsCaseInsensitive()
    {
        var matches = AutocompleteFilter.Filter(Commands, "/FEAT");

        Assert.HasCount(1, matches);
        Assert.AreEqual("features", matches[0].Name);
    }

    [TestMethod]
    public void UnmatchedPrefix_ReturnsNothing()
    {
        Assert.IsEmpty(AutocompleteFilter.Filter(Commands, "/zzz"));
    }
}
