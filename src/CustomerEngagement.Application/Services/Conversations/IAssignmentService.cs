namespace CustomerEngagement.Application.Services.Conversations;

public interface IAssignmentService
{
    Task AssignToAgentAsync(long conversationId, int agentId, CancellationToken cancellationToken = default);

    Task AssignToTeamAsync(long conversationId, int teamId, CancellationToken cancellationToken = default);

    Task AutoAssignAsync(long conversationId, int inboxId, CancellationToken cancellationToken = default);
}
