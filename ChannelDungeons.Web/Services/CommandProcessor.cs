using System.Net;
using ChannelDungeons.Web.Models;

namespace ChannelDungeons.Web.Services;

public enum CommandResultKind
{
    /// <summary>Input was empty or whitespace; nothing to do.</summary>
    None,

    /// <summary>Input was a known slash command; navigate to that channel.</summary>
    SwitchChannel,

    /// <summary>Input warrants a bot reply (unknown command or plain text).</summary>
    BotReply
}

public sealed record CommandResult(CommandResultKind Kind, string? ChannelId = null, string? ReplyHtml = null)
{
    public static readonly CommandResult None = new(CommandResultKind.None);

    public static CommandResult Switch(string channelId) =>
        new(CommandResultKind.SwitchChannel, ChannelId: channelId);

    public static CommandResult Reply(string html) =>
        new(CommandResultKind.BotReply, ReplyHtml: html);
}

/// <summary>Interprets command-box input, mirroring the original site's behavior.</summary>
public static class CommandProcessor
{
    public static CommandResult Process(string input, IReadOnlyList<CommandSuggestion> commands)
    {
        var trimmed = input.Trim();
        if (trimmed.Length == 0)
        {
            return CommandResult.None;
        }

        if (!trimmed.StartsWith('/'))
        {
            return CommandResult.Reply(
                "This is a demonstration of a Discord-like interface. " +
                "Try using commands like <span class='discord-command'>about</span> to navigate.");
        }

        var name = trimmed[1..].ToLowerInvariant();
        if (commands.Any(c => c.Name == name))
        {
            return CommandResult.Switch(name);
        }

        // The reply is rendered as raw HTML, so the echoed input must be
        // encoded. The command list omits the leading slash because the
        // .discord-command style already prepends one via CSS.
        var commandList = string.Join(", ",
            commands.Select(c => $"<span class='discord-command'>{c.Name}</span>"));
        return CommandResult.Reply(
            $"Unknown command: /{WebUtility.HtmlEncode(name)}. Available commands are: {commandList}");
    }
}
