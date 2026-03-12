namespace ChannelDungeons.BlazorWasm.Services;

/// <summary>
/// Service for determining sidebar visibility based on animation progress.
/// </summary>
public static class SidebarAnimationService
{
    /// <summary>
    /// Returns true when the current message is the last one in the sequence,
    /// indicating the sidebar should be revealed.
    /// </summary>
    public static bool ShouldShowSidebar(int currentMessageIndex, int totalMessages)
    {
        if (totalMessages <= 0)
            return false;

        return currentMessageIndex == totalMessages - 1;
    }
}
