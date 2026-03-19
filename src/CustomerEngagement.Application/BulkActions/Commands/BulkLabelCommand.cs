using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.BulkActions.Commands;

public record BulkLabelCommand(
    long AccountId,
    IReadOnlyList<long> ConversationIds,
    IReadOnlyList<int> LabelIds) : IRequest<BulkActionResult>;

public class BulkLabelCommandHandler : IRequestHandler<BulkLabelCommand, BulkActionResult>
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Label> _labelRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkLabelCommandHandler(
        IRepository<Conversation> conversationRepository,
        IRepository<Label> labelRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _labelRepository = labelRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkActionResult> Handle(BulkLabelCommand request, CancellationToken cancellationToken)
    {
        var ids = request.ConversationIds.Select(id => (int)id).ToList();
        var conversations = await _conversationRepository.FindAsync(
            c => ids.Contains(c.Id) && c.AccountId == (int)request.AccountId,
            cancellationToken);

        var labels = await _labelRepository.FindAsync(
            l => request.LabelIds.Contains(l.Id) && l.AccountId == (int)request.AccountId,
            cancellationToken);

        foreach (var conversation in conversations)
        {
            foreach (var label in labels)
            {
                if (!conversation.Labels.Any(l => l.Id == label.Id))
                {
                    conversation.Labels.Add(label);
                }
            }
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new BulkActionResult(conversations.Count);
    }
}
