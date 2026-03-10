using ChannelDungeons.BlazorWasm.Models;

namespace ChannelDungeons.BlazorWasm.Services;

/// <summary>
/// Service for orchestrating message animations with typing indicators.
/// Implements proper cleanup with CancellationToken to prevent memory leaks.
/// </summary>
public class MessageAnimationService
{
    /// <summary>
    /// Animates messages by showing typing indicator, then revealing message.
    /// Uses CancellationToken to allow cleanup when navigating away.
    /// </summary>
    /// <param name="messages">Messages to animate</param>
    /// <param name="onMessageAdded">Callback when message should be added to display</param>
    /// <param name="onTypingIndicatorChanged">Callback to show/hide typing indicator</param>
    /// <param name="config">Animation configuration (timing values)</param>
    /// <param name="cancellationToken">Token to cancel animation</param>
    public async Task AnimateMessagesAsync(
        List<Message> messages,
        Func<int, Task> onMessageAdded,
        Func<bool, Task> onTypingIndicatorChanged,
        AppConfig config,
        CancellationToken cancellationToken = default,
        Func<int, CancellationToken, Task>? delay = null)
    {
        delay ??= Task.Delay;
        try
        {
            int cumulativeDelay = 0;

            for (int i = 0; i < messages.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var message = messages[i];
                var typingDuration = message.TypingDuration ?? config.DefaultTypingDuration;
                var messageDelay = message.Delay ?? config.DefaultMessageDelay;

                // Add initial delay before showing typing indicator
                if (cumulativeDelay > 0)
                {
                    await delay(cumulativeDelay, cancellationToken);
                }

                // Show typing indicator
                await onTypingIndicatorChanged(true);

                // Wait for "typing" duration
                await delay(typingDuration, cancellationToken);

                // Hide typing, show message
                await onTypingIndicatorChanged(false);
                await onMessageAdded(i);

                // Update cumulative delay for next message
                cumulativeDelay = messageDelay;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when navigating away - cleanup is handled by CancellationToken
        }
    }
}
