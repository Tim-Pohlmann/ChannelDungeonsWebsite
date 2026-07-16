using ChannelDungeons.Web.Models;

namespace ChannelDungeons.Web.Data;

/// <summary>
/// Static channel content for the demo. Mirrors the original site's HTML &lt;template&gt; definitions.
/// </summary>
public static class ChannelCatalog
{
    public const string DefaultChannel = "welcome";

    public static readonly IReadOnlyList<ChannelInfo> Channels = new List<ChannelInfo>
    {
        new()
        {
            Name = "welcome",
            Description = "Welcome to Channel Dungeons!",
            Messages = new List<ChannelMessage>
            {
                new()
                {
                    TypingDurationMs = 800,
                    DelayMs = 300,
                    ContentHtml = """
                        <h1>Welcome to Channel Dungeons!</h1>
                        <p>Dive into a fantastic MMORPG adventure with deep dungeons, dangerous monsters and epic loot...</p>
                        """
                },
                new()
                {
                    TypingDurationMs = 700,
                    DelayMs = 1200,
                    ContentHtml = "<p>...and all of it inside Discord!</p>"
                },
                new()
                {
                    TypingDurationMs = 1800,
                    DelayMs = 900,
                    ContentHtml = "<p>Each server is a town and all towns are connected via a sprawling network of dungeons.</p>"
                },
                new()
                {
                    TypingDurationMs = 2500,
                    DelayMs = 1100,
                    ContentHtml = """
                        <p>Explore, fight, level up and meet other players!</p>
                        <p>No download required. Simply start playing in Discord now:</p>
                        <div class="cta-buttons">
                            <a href="https://discord.gg/channeldungeons" class="discord-button">Join the official server</a>
                            <a href="https://discord.com/oauth2/authorize?client_id=YOUR_CLIENT_ID&scope=bot&permissions=YOUR_PERMISSIONS" class="discord-button">Add Channel Dungeons to your own server</a>
                        </div>
                        """
                }
            }
        },
        new()
        {
            Name = "about",
            Description = "More about Channel Dungeons",
            Messages = new List<ChannelMessage>
            {
                new()
                {
                    TypingDurationMs = 800,
                    DelayMs = 300,
                    ContentHtml = """
                        <p>Channel Dungeons is an MMORPG played entirely in Discord, emphasizing exploration and collaboration.</p>
                        <p>Delve into dungeons, battle fearsome monsters, discover valuable loot, and level up your hero!</p>
                        <p>Experience a shared, persistent world where you can encounter other players from other Discord servers while exploring dungeons solo or with friends.</p>
                        """
                },
                new()
                {
                    TypingDurationMs = 1500,
                    DelayMs = 900,
                    ContentHtml = """
                        <p>Each server that wants to participate needs to add the Channel Dungeons application.</p>
                        <div class="cta-buttons">
                            <a href="https://discord.com/oauth2/authorize?client_id=YOUR_CLIENT_ID&scope=bot&permissions=YOUR_PERMISSIONS" class="discord-button">Add Channel Dungeons to your Discord server</a>
                        </div>
                        """
                },
                new()
                {
                    TypingDurationMs = 1800,
                    DelayMs = 850,
                    ContentHtml = """
                        <p>For each server, a town is created in an interconnected world.</p>
                        <p>Each member of the server can create a hero and use the town as their starting point for exploring the world of Channel Dungeons.</p>
                        """
                },
                new()
                {
                    TypingDurationMs = 1200,
                    DelayMs = 800,
                    ContentHtml = """
                        <p>Join the adventure and start your journey today!</p>
                        <div class="cta-buttons">
                            <a href="https://discord.gg/channeldungeons" class="discord-button">Join the official Cahnnel Dungeons server</a>
                        </div>
                        """
                }
            }
        },
        new()
        {
            Name = "features",
            Description = "Gameplay Features",
            Messages = new List<ChannelMessage>
            {
                new()
                {
                    TypingDurationMs = 800,
                    DelayMs = 300,
                    ContentHtml = """
                        <div class="feature">
                            <h3>Shared World</h3>
                            <p>Venture into a persistent world, seamlessly shared across many Discord servers, creating a truly massive multiplayer experience.</p>
                        </div>
                        """
                },
                new()
                {
                    TypingDurationMs = 800,
                    DelayMs = 600,
                    ContentHtml = """
                        <div class="feature">
                            <h3>Dungeon Crawl</h3>
                            <p>Fight monsters, earn experience, and level up!</p>
                        </div>
                        """
                },
                new()
                {
                    TypingDurationMs = 1000,
                    DelayMs = 650,
                    ContentHtml = """
                        <div class="feature">
                            <h3>Multiplayer</h3>
                            <p>Forge alliances with other players and embark on thrilling adventures together!</p>
                        </div>
                        """
                },
                new()
                {
                    TypingDurationMs = 1400,
                    DelayMs = 700,
                    ContentHtml = """
                        <div class="feature">
                            <h3>Loot</h3>
                            <p>Loot, loot loot! So much loot!</p>
                            <p>Find powerful equipment to fight tougher foes who drop even better loot!</p>
                        </div>
                        """
                }
            }
        },
        new()
        {
            Name = "gameplay-demo",
            Description = "A brief demonstration of how Channel Dungeons plays",
            Messages = new List<ChannelMessage>
            {
                new()
                {
                    TypingDurationMs = 800,
                    DelayMs = 300,
                    ContentHtml = """
                        <h2>Gameplay Demo</h2>
                        <p>This channel will show you what gameplay looks like in Channel Dungeons.</p>
                        <p>Coming soon!</p>
                        """
                }
            }
        }
    };

    public static ChannelInfo? Find(string name) =>
        Channels.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.Ordinal));

    public static bool IsValidChannel(string name) => Find(name) is not null;

    /// <summary>Sidebar order: "welcome" first, then alphabetical.</summary>
    public static IEnumerable<ChannelInfo> InSidebarOrder() =>
        Channels.OrderBy(c => c.Name == DefaultChannel ? string.Empty : c.Name, StringComparer.Ordinal);
}
