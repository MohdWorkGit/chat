using MediatR;

namespace CustomerEngagement.Application.Drafts.Commands;

public record SaveDraftCommand(
    int ConversationId,
    int AccountId,
    int UserId,
    string Content,
    string ContentType = "text") : IRequest<DraftResponse>;

public record DraftResponse(int Id, int ConversationId, int UserId, string? Content, string? ContentType, DateTime UpdatedAt);

public class SaveDraftCommandHandler : IRequestHandler<SaveDraftCommand, DraftResponse>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.ConversationDraft> _draftRepository;
    private readonly Core.Interfaces.IUnitOfWork _unitOfWork;

    public SaveDraftCommandHandler(
        Core.Interfaces.IRepository<Core.Entities.ConversationDraft> draftRepository,
        Core.Interfaces.IUnitOfWork unitOfWork)
    {
        _draftRepository = draftRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DraftResponse> Handle(SaveDraftCommand request, CancellationToken cancellationToken)
    {
        // Find existing draft for this user+conversation
        var existing = await _draftRepository.FindOneAsync(d =>
            d.ConversationId == request.ConversationId &&
            d.UserId == request.UserId &&
            d.AccountId == request.AccountId, cancellationToken);

        if (existing is not null)
        {
            existing.Content = request.Content;
            existing.ContentType = request.ContentType;
            existing.UpdatedAt = DateTime.UtcNow;
            _draftRepository.Update(existing);
        }
        else
        {
            existing = new Core.Entities.ConversationDraft
            {
                ConversationId = request.ConversationId,
                AccountId = request.AccountId,
                UserId = request.UserId,
                Content = request.Content,
                ContentType = request.ContentType
            };
            await _draftRepository.AddAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DraftResponse(existing.Id, existing.ConversationId, existing.UserId,
            existing.Content, existing.ContentType, existing.UpdatedAt);
    }
}

public record DeleteDraftCommand(int ConversationId, int AccountId, int UserId) : IRequest;

public class DeleteDraftCommandHandler : IRequestHandler<DeleteDraftCommand>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.ConversationDraft> _draftRepository;
    private readonly Core.Interfaces.IUnitOfWork _unitOfWork;

    public DeleteDraftCommandHandler(
        Core.Interfaces.IRepository<Core.Entities.ConversationDraft> draftRepository,
        Core.Interfaces.IUnitOfWork unitOfWork)
    {
        _draftRepository = draftRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDraftCommand request, CancellationToken cancellationToken)
    {
        var existing = await _draftRepository.FindOneAsync(d =>
            d.ConversationId == request.ConversationId &&
            d.UserId == request.UserId &&
            d.AccountId == request.AccountId, cancellationToken);

        if (existing is not null)
        {
            _draftRepository.Remove(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
