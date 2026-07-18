# Channel Dungeons Website

This is the official website for Channel Dungeons, an MMORPG played entirely in Discord. The game emphasizes exploration and collaboration. Delve into dungeons, battle fearsome monsters, discover valuable loot, and level up your hero!

## Play Now

- [Join the official Channel Dungeons discord server](https://discord.gg/channeldungeons)
- [Add the bot to your own server](https://discord.com/oauth2/authorize?client_id=YOUR_CLIENT_ID&scope=bot&permissions=YOUR_PERMISSIONS)

## Development

The site is a .NET 10 Blazor WebAssembly app presenting a Discord-like interface. Channel content lives in `ChannelDungeons.Web/Services/ChannelCatalog.cs`.

```bash
dotnet run --project ChannelDungeons.Web    # run locally
dotnet test                                 # run the test suite
```

Pushes to `main` are deployed to [channel-dungeons.com](https://channel-dungeons.com) via GitHub Actions (`.github/workflows/deploy.yml`). CI (`.github/workflows/ci.yml`) builds, tests, and runs SonarQube Cloud analysis on every PR.
