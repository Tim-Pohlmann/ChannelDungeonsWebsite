using System.Net.Http.Json;
using ChannelDungeons.BlazorWasm.Models;

namespace ChannelDungeons.BlazorWasm.Services;

/// <summary>
/// Service for loading and managing channel data from the channels.json configuration file.
/// Implements message caching to avoid re-animation when revisiting channels.
/// </summary>
public class ChannelService
{
    private readonly HttpClient _httpClient;
    private ChannelData? _channelData;
    private readonly Dictionary<string, List<Message>> _messageCache = new();

    public ChannelService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets all available channels, loading from JSON if not already cached.
    /// </summary>
    /// <exception cref="HttpRequestException">Thrown if JSON file cannot be loaded</exception>
    public async Task<List<Channel>> GetAllChannelsAsync()
    {
        if (_channelData == null)
        {
            try
            {
                _channelData = await _httpClient.GetFromJsonAsync<ChannelData>("data/channels.json");
                if (_channelData == null)
                {
                    throw new HttpRequestException("Failed to deserialize channels.json");
                }
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Failed to load channels.json: {ex.Message}", ex);
            }
        }

        return _channelData.Channels;
    }

    /// <summary>
    /// Gets a specific channel by ID.
    /// </summary>
    /// <param name="channelId">The channel ID to retrieve</param>
    /// <returns>The channel, or null if not found</returns>
    public async Task<Channel?> GetChannelAsync(string channelId)
    {
        var channels = await GetAllChannelsAsync();
        return channels.FirstOrDefault(c => c.Id.Equals(channelId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the global animation configuration.
    /// </summary>
    public async Task<AppConfig> GetConfigAsync()
    {
        if (_channelData == null)
        {
            await GetAllChannelsAsync();
        }

        return _channelData?.Config ?? new AppConfig();
    }

    /// <summary>
    /// Gets all available channel IDs for command completion.
    /// </summary>
    public async Task<List<string>> GetChannelCommandsAsync()
    {
        var channels = await GetAllChannelsAsync();
        return channels.Select(c => $"/{c.Name}").ToList();
    }

    /// <summary>
    /// Checks if messages for a channel have been cached (previously viewed).
    /// </summary>
    /// <param name="channelId">The channel ID to check</param>
    /// <returns>True if messages are cached, false otherwise</returns>
    public bool IsCached(string channelId)
    {
        return _messageCache.ContainsKey(channelId);
    }

    /// <summary>
    /// Gets cached messages for a channel if available.
    /// </summary>
    /// <param name="channelId">The channel ID</param>
    /// <returns>List of cached messages, or null if not cached</returns>
    public List<Message>? GetCachedMessages(string channelId)
    {
        _messageCache.TryGetValue(channelId, out var messages);
        return messages;
    }

    /// <summary>
    /// Caches messages for a channel after animation completes.
    /// </summary>
    /// <param name="channelId">The channel ID</param>
    /// <param name="messages">The messages to cache</param>
    public void CacheMessages(string channelId, List<Message> messages)
    {
        _messageCache[channelId] = messages;
    }

    /// <summary>
    /// Clears the message cache (useful for testing).
    /// </summary>
    public void ClearCache()
    {
        _messageCache.Clear();
    }
}
