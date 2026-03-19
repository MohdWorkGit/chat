using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.BulkActions.Commands;

public record BulkAssignCommand(
    long AccountId,
    IReadOnlyList<long> ConversationIds,
    int? AssigneeId,
    int? TeamId) : IRequest<BulkActionResult>;

public record BulkActionResult(int AffectedCount);

public class BulkAssignCommandHandler : IRequestHandler<BulkAssignCommand, BulkActionResult>
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkAssignCommandHandler(IRepository<Conversation> conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkActionResult> Handle(BulkAssignCommand request, CancellationToken cancellationToken)
    {
        var ids = request.ConversationIds.Select(id => (int)id).ToList();
        var conversations = await _conversationRepository.FindAsync(
            c => ids.Contains(c.Id) && c.AccountId == (int)request.AccountId,
            cancellationToken);

        foreach (var conversation in conversations)
        {
            if (request.AssigneeId.HasValue)
                conversation.AssigneeId = request.AssigneeId.Value;
            if (request.TeamId.HasValue)
                conversation.TeamId = request.TeamId.Value;
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new BulkActionResult(conversations.Count);
    }
}
