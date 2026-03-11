namespace ChannelDungeons.BlazorWasm.Services;

/// <summary>
/// Service for managing sidebar animation timing and visibility state.
/// Determines when the sidebar should become visible during message animations.
/// </summary>
public class SidebarAnimationService
{
    /// <summary>
    /// Determines if the sidebar should be shown based on the current message index.
    /// Returns true when the last message in the sequence is reached.
    /// </summary>
    /// <param name="currentMessageIndex">Zero-based index of the current message</param>
    /// <param name="totalMessages">Total number of messages in the sequence</param>
    /// <returns>True if sidebar should become visible</returns>
    public bool ShouldShowSidebar(int currentMessageIndex, int totalMessages)
    {
        return currentMessageIndex == totalMessages - 1 && totalMessages > 0;
    }
}
