using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.BulkActions.Commands;

public record BulkResolveCommand(
    long AccountId,
    IReadOnlyList<long> ConversationIds) : IRequest<BulkActionResult>;

public class BulkResolveCommandHandler : IRequestHandler<BulkResolveCommand, BulkActionResult>
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkResolveCommandHandler(IRepository<Conversation> conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkActionResult> Handle(BulkResolveCommand request, CancellationToken cancellationToken)
    {
        var ids = request.ConversationIds.Select(id => (int)id).ToList();
        var conversations = await _conversationRepository.FindAsync(
            c => ids.Contains(c.Id) && c.AccountId == (int)request.AccountId,
            cancellationToken);

        var resolved = 0;
        foreach (var conversation in conversations)
        {
            if (conversation.Status != ConversationStatus.Resolved)
            {
                conversation.Status = ConversationStatus.Resolved;
                conversation.UpdatedAt = DateTime.UtcNow;
                resolved++;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new BulkActionResult(resolved);
    }
}
