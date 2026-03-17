using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Conversations;

public interface ITypingStatusManager
{
    Task SetTypingAsync(int conversationId, int userId, bool isTyping, CancellationToken cancellationToken = default);
    Task<List<int>> GetTypingUsersAsync(int conversationId, CancellationToken cancellationToken = default);
    Task ClearTypingAsync(int conversationId, int userId, CancellationToken cancellationToken = default);
}

public class TypingStatusManager : ITypingStatusManager
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<TypingStatusManager> _logger;

    private static readonly TimeSpan TypingTtl = TimeSpan.FromSeconds(10);
    private const string KeyPrefix = "typing";

    public TypingStatusManager(
        IDistributedCache cache,
        ILogger<TypingStatusManager> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SetTypingAsync(
        int conversationId,
        int userId,
        bool isTyping,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(conversationId, userId);

        if (isTyping)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TypingTtl
            };

            var value = JsonSerializer.Serialize(new TypingEntry
            {
                UserId = userId,
                ConversationId = conversationId,
                Timestamp = DateTime.UtcNow
            });

            await _cache.SetStringAsync(key, value, options, cancellationToken);

            // Add user to conversation tracking list
            var trackingKey = BuildConversationTrackingKey(conversationId);
            var trackingData = await _cache.GetStringAsync(trackingKey, cancellationToken);
            var trackedUserIds = string.IsNullOrEmpty(trackingData)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(trackingData) ?? [];

            if (!trackedUserIds.Contains(userId))
            {
                trackedUserIds.Add(userId);
            }

            await UpdateTrackingKeyAsync(conversationId, trackedUserIds, cancellationToken);

            _logger.LogDebug("Set typing status for user {UserId} in conversation {ConversationId}", userId, conversationId);
        }
        else
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cleared typing status for user {UserId} in conversation {ConversationId}", userId, conversationId);
        }
    }

    public async Task<List<int>> GetTypingUsersAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the list of tracked user keys for this conversation
        var trackingKey = BuildConversationTrackingKey(conversationId);
        var trackingData = await _cache.GetStringAsync(trackingKey, cancellationToken);

        if (string.IsNullOrEmpty(trackingData))
            return [];

        var trackedUserIds = JsonSerializer.Deserialize<List<int>>(trackingData) ?? [];
        var typingUsers = new List<int>();

        foreach (var userId in trackedUserIds)
        {
            var key = BuildKey(conversationId, userId);
            var value = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(value))
            {
                typingUsers.Add(userId);
            }
        }

        // Clean up tracking key if no users are typing
        if (typingUsers.Count == 0)
        {
            await _cache.RemoveAsync(trackingKey, cancellationToken);
        }
        else if (typingUsers.Count != trackedUserIds.Count)
        {
            // Update tracking with only active typing users
            await UpdateTrackingKeyAsync(conversationId, typingUsers, cancellationToken);
        }

        return typingUsers;
    }

    public async Task ClearTypingAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(conversationId, userId);
        await _cache.RemoveAsync(key, cancellationToken);

        // Remove user from tracking
        var trackingKey = BuildConversationTrackingKey(conversationId);
        var trackingData = await _cache.GetStringAsync(trackingKey, cancellationToken);

        if (!string.IsNullOrEmpty(trackingData))
        {
            var trackedUserIds = JsonSerializer.Deserialize<List<int>>(trackingData) ?? [];
            trackedUserIds.Remove(userId);

            if (trackedUserIds.Count == 0)
            {
                await _cache.RemoveAsync(trackingKey, cancellationToken);
            }
            else
            {
                await UpdateTrackingKeyAsync(conversationId, trackedUserIds, cancellationToken);
            }
        }

        _logger.LogDebug("Cleared typing status for user {UserId} in conversation {ConversationId}", userId, conversationId);
    }

    private async Task UpdateTrackingKeyAsync(
        int conversationId,
        List<int> userIds,
        CancellationToken cancellationToken)
    {
        var trackingKey = BuildConversationTrackingKey(conversationId);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TypingTtl
        };

        await _cache.SetStringAsync(
            trackingKey,
            JsonSerializer.Serialize(userIds),
            options,
            cancellationToken);
    }

    private static string BuildKey(int conversationId, int userId)
        => $"{KeyPrefix}:{conversationId}:{userId}";

    private static string BuildConversationTrackingKey(int conversationId)
        => $"{KeyPrefix}:{conversationId}:_tracking";

    private class TypingEntry
    {
        public int UserId { get; set; }
        public int ConversationId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
