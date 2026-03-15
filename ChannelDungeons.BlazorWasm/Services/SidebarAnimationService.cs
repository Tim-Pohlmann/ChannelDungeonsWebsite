namespace ChannelDungeons.BlazorWasm.Services;

/// <summary>
/// Service for managing sidebar animation timing and visibility state.
/// Determines when the sidebar should become visible during message animations.
/// </summary>
public static class SidebarAnimationService
{
    private const string WelcomeChannelId = "welcome";

    /// <summary>
    /// Determines if the sidebar should be shown based on the current message index.
    /// Returns true only for the welcome channel when the last message is reached
    /// and the sidebar has not already been revealed.
    /// </summary>
    /// <param name="currentMessageIndex">Zero-based index of the current message</param>
    /// <param name="totalMessages">Total number of messages in the sequence</param>
    /// <param name="channelId">The channel being animated</param>
    /// <param name="sidebarRevealDone">Whether the sidebar has already been revealed</param>
    /// <returns>True if sidebar should become visible</returns>
    public static bool ShouldShowSidebar(int currentMessageIndex, int totalMessages, string channelId, bool sidebarRevealDone)
    {
        return !sidebarRevealDone
            && channelId.Equals(WelcomeChannelId, StringComparison.OrdinalIgnoreCase)
            && currentMessageIndex == totalMessages - 1
            && totalMessages > 0;
    }
}
