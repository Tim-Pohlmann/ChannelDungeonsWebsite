using ChannelDungeons.Web.Models;

namespace ChannelDungeons.Web.Services;

public interface IChannelCatalog
{
    /// <summary>Channels in sidebar order: welcome first, the rest alphabetical.</summary>
    IReadOnlyList<Channel> Channels { get; }

    /// <summary>Commands offered by the input box, in the same order as the sidebar.</summary>
    IReadOnlyList<CommandSuggestion> Commands { get; }

    bool TryGetChannel(string id, out Channel channel);
}

/// <summary>
/// All site content in one place, mirroring the channel templates of the
/// original static site. Message HTML is trusted, compiled-in markup.
/// </summary>
public sealed class ChannelCatalog : IChannelCatalog
{
    public const string DefaultChannelId = "welcome";

    private const string DiscordInviteUrl = "https://discord.gg/channeldungeons";
    private const string BotInstallUrl = "https://discord.com/oauth2/authorize?client_id=YOUR_CLIENT_ID&scope=bot&permissions=YOUR_PERMISSIONS";

    private readonly Dictionary<string, Channel> _channelsById;

    public ChannelCatalog()
    {
        Channels = SortForSidebar(CreateChannels());
        _channelsById = Channels.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
        Commands = Channels.Select(c => new CommandSuggestion(c.Id, c.Description)).ToArray();
    }

    public IReadOnlyList<Channel> Channels { get; }

    public IReadOnlyList<CommandSuggestion> Commands { get; }

    public bool TryGetChannel(string id, out Channel channel) =>
        _channelsById.TryGetValue(id, out channel!);

    private static Channel[] SortForSidebar(IEnumerable<Channel> channels) =>
        channels
            .OrderBy(c => c.Id == DefaultChannelId ? 0 : 1)
            .ThenBy(c => c.Id, StringComparer.Ordinal)
            .ToArray();

    private static IEnumerable<Channel> CreateChannels()
    {
        yield return new Channel(
            DefaultChannelId,
            "Welcome to Channel Dungeons!",
            [
                new ChannelMessage(
                    """
                    <h1>Welcome to Channel Dungeons!</h1>
                    <p>Dive into a fantastic MMORPG adventure with deep dungeons, dangerous monsters and epic loot...</p>
                    """,
                    TypingDurationMs: 800, DelayMs: 300),
                new ChannelMessage(
                    "<p>...and all of it inside Discord!</p>",
                    TypingDurationMs: 700, DelayMs: 1200),
                new ChannelMessage(
                    "<p>Each server is a town and all towns are connected via a sprawling network of dungeons.</p>",
                    TypingDurationMs: 1800, DelayMs: 900),
                new ChannelMessage(
                    $"""
                    <p>Explore, fight, level up and meet other players!</p>
                    <p>No download required. Simply start playing in Discord now:</p>
                    <div class="cta-buttons">
                        <a href="{DiscordInviteUrl}" class="discord-button">Join the official server</a>
                        <a href="{BotInstallUrl}" class="discord-button">Add Channel Dungeons to your own server</a>
                    </div>
                    """,
                    TypingDurationMs: 2500, DelayMs: 1100)
            ]);

        yield return new Channel(
            "about",
            "More about Channel Dungeons",
            [
                new ChannelMessage(
                    """
                    <p>Channel Dungeons is an MMORPG played entirely in Discord, emphasizing exploration and collaboration.</p>
                    <p>Delve into dungeons, battle fearsome monsters, discover valuable loot, and level up your hero!</p>
                    <p>Experience a shared, persistent world where you can encounter other players from other Discord servers while exploring dungeons solo or with friends.</p>
                    """,
                    TypingDurationMs: 800, DelayMs: 300),
                new ChannelMessage(
                    $"""
                    <p>Each server that wants to participate needs to add the Channel Dungeons application.</p>
                    <div class="cta-buttons">
                        <a href="{BotInstallUrl}" class="discord-button">Add Channel Dungeons to your Discord server</a>
                    </div>
                    """,
                    TypingDurationMs: 1500, DelayMs: 900),
                new ChannelMessage(
                    """
                    <p>For each server, a town is created in an interconnected world.</p>
                    <p>Each member of the server can create a hero and use the town as their starting point for exploring the world of Channel Dungeons.</p>
                    """,
                    TypingDurationMs: 1800, DelayMs: 850),
                new ChannelMessage(
                    $"""
                    <p>Join the adventure and start your journey today!</p>
                    <div class="cta-buttons">
                        <a href="{DiscordInviteUrl}" class="discord-button">Join the official Channel Dungeons server</a>
                    </div>
                    """,
                    TypingDurationMs: 1200, DelayMs: 800)
            ]);

        yield return new Channel(
            "features",
            "Gameplay Features",
            [
                new ChannelMessage(
                    """
                    <div class="feature">
                        <h3>Shared World</h3>
                        <p>Venture into a persistent world, seamlessly shared across many Discord servers, creating a truly massive multiplayer experience.</p>
                    </div>
                    """,
                    TypingDurationMs: 800, DelayMs: 300),
                new ChannelMessage(
                    """
                    <div class="feature">
                        <h3>Dungeon Crawl</h3>
                        <p>Fight monsters, earn experience, and level up!</p>
                    </div>
                    """,
                    TypingDurationMs: 800, DelayMs: 600),
                new ChannelMessage(
                    """
                    <div class="feature">
                        <h3>Multiplayer</h3>
                        <p>Forge alliances with other players and embark on thrilling adventures together!</p>
                    </div>
                    """,
                    TypingDurationMs: 1000, DelayMs: 650),
                new ChannelMessage(
                    """
                    <div class="feature">
                        <h3>Loot</h3>
                        <p>Loot, loot loot! So much loot!</p>
                        <p>Find powerful equipment to fight tougher foes who drop even better loot!</p>
                    </div>
                    """,
                    TypingDurationMs: 1400, DelayMs: 700)
            ]);

        yield return new Channel(
            "gameplay-demo",
            "A brief demonstration of how Channel Dungeons plays",
            [
                new ChannelMessage(
                    """
                    <h2>Gameplay Demo</h2>
                    <p>This channel will show you what gameplay looks like in Channel Dungeons.</p>
                    <p>Coming soon!</p>
                    """,
                    TypingDurationMs: 800, DelayMs: 300)
            ]);
    }
}
