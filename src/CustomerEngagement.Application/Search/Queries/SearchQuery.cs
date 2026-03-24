using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using CustomerEngagement.Application.Services.Conversations;
using MediatR;

namespace CustomerEngagement.Application.Search.Queries;

public record SearchQuery(
    int AccountId,
    string Query,
    int Page = 1,
    int PageSize = 25) : IRequest<SearchResultDto>;

public record SearchResultDto(
    IReadOnlyList<ConversationDto> Conversations,
    IReadOnlyList<ContactDto> Contacts,
    IReadOnlyList<MessageDto> Messages);

public class SearchQueryHandler : IRequestHandler<SearchQuery, SearchResultDto>
{
    private readonly IConversationService _conversationService;
    private readonly IContactService _contactService;

    public SearchQueryHandler(
        IConversationService conversationService,
        IContactService contactService)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
    }

    public async Task<SearchResultDto> Handle(SearchQuery request, CancellationToken cancellationToken)
    {
        var conversationFilter = new ConversationFilterDto
        {
            Query = request.Query
        };

        var conversationsTask = _conversationService.GetByAccountAsync(
            request.AccountId, conversationFilter, request.Page, request.PageSize, cancellationToken);

        var contactsTask = _contactService.SearchAsync(
            request.AccountId, request.Query, request.Page, request.PageSize, cancellationToken);

        await Task.WhenAll(conversationsTask, contactsTask);

        var conversations = (await conversationsTask).Items.ToList().AsReadOnly();
        var contacts = (await contactsTask).Items.ToList().AsReadOnly();

        return new SearchResultDto(
            conversations,
            contacts,
            Array.Empty<MessageDto>().AsReadOnly());
    }
}
