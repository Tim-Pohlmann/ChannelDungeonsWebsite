using ChannelDungeons.BlazorWasm.Models;

namespace ChannelDungeons.BlazorWasm.Services;

/// <summary>
/// Service for orchestrating message animations with typing indicators.
/// Implements proper cleanup with CancellationToken to prevent memory leaks.
/// </summary>
public class MessageAnimationService
{
    private readonly Func<int, CancellationToken, Task> _delay;

    public MessageAnimationService() : this(Task.Delay) { }

    public MessageAnimationService(Func<int, CancellationToken, Task> delay)
    {
        _delay = delay;
    }

    /// <summary>
    /// Animates messages by showing typing indicator, then revealing message.
    /// Uses cumulative timing matching the original vanilla JS behavior:
    /// each message's delay is an additional pause on top of the previous message's
    /// completion time, plus the configured between-message gap.
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var message = messages[i];
                var typingDuration = message.TypingDuration ?? config.DefaultTypingDuration;
                var additionalDelay = message.Delay ?? 0;

                // Wait for between-message gap (skip for first message) plus any per-message extra delay
                var preDelay = (i > 0 ? config.DefaultMessageDelay : 0) + additionalDelay;
                if (preDelay > 0)
                {
                    await _delay(preDelay, cancellationToken);
                }

                // Show typing indicator
                await onTypingIndicatorChanged(true);

                // Wait for "typing" duration
                await _delay(typingDuration, cancellationToken);

                // Hide typing, show message
                await onTypingIndicatorChanged(false);
                await onMessageAdded(i);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when navigating away - cleanup is handled by CancellationToken
        }
    }
}
