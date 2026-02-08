namespace ChannelDungeons.BlazorWasm.Models;

/// <summary>
/// Global animation and timing configuration for the application.
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Default duration in milliseconds to show typing indicator.
    /// </summary>
    public int DefaultTypingDuration { get; set; } = 1000;

    /// <summary>
    /// Default delay in milliseconds between messages.
    /// </summary>
    public int DefaultMessageDelay { get; set; } = 200;

    /// <summary>
    /// Delay in milliseconds to show UI elements on first visit.
    /// </summary>
    public int UiShowDelay { get; set; } = 500;
}
