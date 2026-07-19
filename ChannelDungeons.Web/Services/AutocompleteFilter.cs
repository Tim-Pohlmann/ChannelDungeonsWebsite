using ChannelDungeons.Web.Models;

namespace ChannelDungeons.Web.Services;

/// <summary>Filters command suggestions for the autocomplete dropdown.</summary>
public static class AutocompleteFilter
{
    /// <summary>
    /// Returns suggestions whose name starts with the typed prefix.
    /// Empty unless the input starts with '/'.
    /// </summary>
    public static IReadOnlyList<CommandSuggestion> Filter(
        IReadOnlyList<CommandSuggestion> commands, string input)
    {
        if (!input.StartsWith('/'))
        {
            return [];
        }

        var searchTerm = input[1..];
        return commands
            .Where(c => c.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
