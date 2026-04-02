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
    private readonly Core.Interfaces.IRepository<Core.Entities.Message> _messageRepository;

    public SearchQueryHandler(
        IConversationService conversationService,
        IContactService contactService,
        Core.Interfaces.IRepository<Core.Entities.Message> messageRepository)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
        _messageRepository = messageRepository;
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

        var messagesTask = _messageRepository.FindAsync(
            m => m.AccountId == request.AccountId && m.Content != null && m.Content.Contains(request.Query),
            cancellationToken);

        await Task.WhenAll(conversationsTask, contactsTask, messagesTask);

        var conversations = (await conversationsTask).Items.ToList().AsReadOnly();
        var contacts = (await contactsTask).Items.ToList().AsReadOnly();
        var messageEntities = await messagesTask;
        var messages = messageEntities
            .Take(request.PageSize)
            .Select(m => new MessageDto(
                m.Id, m.ConversationId, m.AccountId, m.SenderId,
                m.SenderType, m.Content, m.ContentType,
                m.MessageType.ToString(), m.Private, m.Status.ToString(),
                m.SentAt, m.CreatedAt,
                Array.Empty<AttachmentDto>().AsReadOnly()))
            .ToList()
            .AsReadOnly();

        return new SearchResultDto(
            conversations,
            contacts,
            messages);
    }
}
