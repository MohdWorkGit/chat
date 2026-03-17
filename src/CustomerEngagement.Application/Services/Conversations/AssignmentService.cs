using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Services.Conversations;

public class AssignmentService : IAssignmentService
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<InboxMember> _inboxMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    // Track last assigned index per inbox for round-robin
    private static readonly Dictionary<int, int> _roundRobinIndex = new();
    private static readonly object _roundRobinLock = new();

    public AssignmentService(
        IRepository<Conversation> conversationRepository,
        IRepository<InboxMember> inboxMemberRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _inboxMemberRepository = inboxMemberRepository ?? throw new ArgumentNullException(nameof(inboxMemberRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task AssignToAgentAsync(long conversationId, int agentId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync((int)conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        conversation.AssigneeId = agentId;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new ConversationAssignedEvent(conversationId, conversation.AccountId, agentId, conversation.TeamId),
            cancellationToken);
    }

    public async Task AssignToTeamAsync(long conversationId, int teamId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync((int)conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        conversation.TeamId = teamId;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new ConversationAssignedEvent(conversationId, conversation.AccountId, conversation.AssigneeId, teamId),
            cancellationToken);
    }

    public async Task AutoAssignAsync(long conversationId, int inboxId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync((int)conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        // Get all available agents for this inbox
        var inboxMembers = await _inboxMemberRepository.ListAsync(
            new { InboxId = inboxId },
            cancellationToken);

        var memberList = inboxMembers.ToList();
        if (memberList.Count == 0)
            return;

        // Round-robin assignment
        int nextAgentId;
        lock (_roundRobinLock)
        {
            if (!_roundRobinIndex.TryGetValue(inboxId, out var currentIndex))
                currentIndex = 0;

            var nextIndex = currentIndex % memberList.Count;
            nextAgentId = memberList[nextIndex].UserId;
            _roundRobinIndex[inboxId] = nextIndex + 1;
        }

        conversation.AssigneeId = nextAgentId;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new ConversationAssignedEvent(conversationId, conversation.AccountId, nextAgentId, conversation.TeamId),
            cancellationToken);
    }
}
