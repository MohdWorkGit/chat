using CustomerEngagement.Application.Drafts.Commands;
using MediatR;

namespace CustomerEngagement.Application.Drafts.Queries;

public record GetDraftQuery(int ConversationId, int AccountId, int UserId) : IRequest<DraftResponse?>;

public class GetDraftQueryHandler : IRequestHandler<GetDraftQuery, DraftResponse?>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.ConversationDraft> _draftRepository;

    public GetDraftQueryHandler(Core.Interfaces.IRepository<Core.Entities.ConversationDraft> draftRepository)
    {
        _draftRepository = draftRepository;
    }

    public async Task<DraftResponse?> Handle(GetDraftQuery request, CancellationToken cancellationToken)
    {
        var draft = await _draftRepository.FindOneAsync(d =>
            d.ConversationId == request.ConversationId &&
            d.UserId == request.UserId &&
            d.AccountId == request.AccountId, cancellationToken);

        if (draft is null)
            return null;

        return new DraftResponse(draft.Id, draft.ConversationId, draft.UserId,
            draft.Content, draft.ContentType, draft.UpdatedAt);
    }
}
