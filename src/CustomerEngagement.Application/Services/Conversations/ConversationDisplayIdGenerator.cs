using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;

namespace CustomerEngagement.Application.Services.Conversations;

/// <summary>
/// Helper for computing the next per-account DisplayId for a conversation.
/// The <c>conversations</c> table has a unique index on (AccountId, DisplayId);
/// new conversations must be assigned an account-scoped sequential DisplayId.
/// </summary>
public static class ConversationDisplayIdGenerator
{
    /// <summary>
    /// Returns the next DisplayId for the given account (max existing + 1, or 1 if none).
    /// </summary>
    public static async Task<int> GetNextDisplayIdAsync(
        IRepository<Conversation> conversationRepository,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        var latest = await conversationRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 1,
            predicate: c => c.AccountId == accountId,
            orderBy: c => c.DisplayId,
            ascending: false,
            cancellationToken: cancellationToken);

        return (latest.Count > 0 ? latest[0].DisplayId : 0) + 1;
    }
}
