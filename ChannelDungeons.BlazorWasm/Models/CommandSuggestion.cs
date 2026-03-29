namespace ChannelDungeons.BlazorWasm.Models;

/// <summary>
/// Represents a command suggestion for the autocomplete dropdown,
/// pairing a slash command name with its description.
/// </summary>
public record CommandSuggestion(string Name, string Description);
