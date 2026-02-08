namespace ChannelDungeons.BlazorWasm.Models;

/// <summary>
/// Root object for deserializing channels.json configuration file.
/// </summary>
public class ChannelData
{
    /// <summary>
    /// Collection of all available channels.
    /// </summary>
    public List<Channel> Channels { get; set; } = new();

    /// <summary>
    /// Global configuration for animations and timing.
    /// </summary>
    public AppConfig Config { get; set; } = new();
}
