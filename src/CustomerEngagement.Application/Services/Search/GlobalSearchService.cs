using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Search;

public interface IGlobalSearchService
{
    Task<GlobalSearchResultDto> SearchAsync(int accountId, string query, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
    Task<GlobalSearchResultDto> SearchConversationsAsync(int accountId, string query, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
    Task<GlobalSearchResultDto> SearchContactsAsync(int accountId, string query, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
    Task<GlobalSearchResultDto> SearchMessagesAsync(int accountId, string query, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
}

public class GlobalSearchService : IGlobalSearchService
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly ILogger<GlobalSearchService> _logger;

    public GlobalSearchService(
        IRepository<Conversation> conversationRepository,
        IRepository<Contact> contactRepository,
        IRepository<Message> messageRepository,
        ILogger<GlobalSearchService> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GlobalSearchResultDto> SearchAsync(
        int accountId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GlobalSearchResultDto.Empty;

        var normalizedQuery = query.Trim().ToLowerInvariant();

        var conversations = await FindConversationsAsync(accountId, normalizedQuery, cancellationToken);
        var contacts = await FindContactsAsync(accountId, normalizedQuery, cancellationToken);
        var messages = await FindMessagesAsync(accountId, normalizedQuery, cancellationToken);

        return new GlobalSearchResultDto
        {
            Conversations = conversations
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            Contacts = contacts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            Messages = messages
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            TotalCount = conversations.Count + contacts.Count + messages.Count
        };
    }

    public async Task<GlobalSearchResultDto> SearchConversationsAsync(
        int accountId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GlobalSearchResultDto.Empty;

        var normalizedQuery = query.Trim().ToLowerInvariant();
        var conversations = await FindConversationsAsync(accountId, normalizedQuery, cancellationToken);

        return new GlobalSearchResultDto
        {
            Conversations = conversations
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            TotalCount = conversations.Count
        };
    }

    public async Task<GlobalSearchResultDto> SearchContactsAsync(
        int accountId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GlobalSearchResultDto.Empty;

        var normalizedQuery = query.Trim().ToLowerInvariant();
        var contacts = await FindContactsAsync(accountId, normalizedQuery, cancellationToken);

        return new GlobalSearchResultDto
        {
            Contacts = contacts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            TotalCount = contacts.Count
        };
    }

    public async Task<GlobalSearchResultDto> SearchMessagesAsync(
        int accountId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GlobalSearchResultDto.Empty;

        var normalizedQuery = query.Trim().ToLowerInvariant();
        var messages = await FindMessagesAsync(accountId, normalizedQuery, cancellationToken);

        return new GlobalSearchResultDto
        {
            Messages = messages
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            TotalCount = messages.Count
        };
    }

    private async Task<List<ConversationSearchResult>> FindConversationsAsync(
        int accountId,
        string query,
        CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.FindAsync(
            c => c.AccountId == accountId
                && (c.Identifier != null && c.Identifier.ToLower().Contains(query)
                    || c.AdditionalAttributes != null && c.AdditionalAttributes.ToLower().Contains(query)),
            cancellationToken);

        return conversations.Select(c => new ConversationSearchResult
        {
            Id = c.Id,
            AccountId = c.AccountId,
            InboxId = c.InboxId,
            ContactId = c.ContactId,
            Identifier = c.Identifier,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    private async Task<List<ContactSearchResult>> FindContactsAsync(
        int accountId,
        string query,
        CancellationToken cancellationToken)
    {
        var contacts = await _contactRepository.FindAsync(
            c => c.AccountId == accountId
                && (c.Name != null && c.Name.ToLower().Contains(query)
                    || c.Email != null && c.Email.ToLower().Contains(query)
                    || c.Phone != null && c.Phone.ToLower().Contains(query)
                    || c.Identifier != null && c.Identifier.ToLower().Contains(query)),
            cancellationToken);

        return contacts.Select(c => new ContactSearchResult
        {
            Id = c.Id,
            AccountId = c.AccountId,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Identifier = c.Identifier,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    private async Task<List<MessageSearchResult>> FindMessagesAsync(
        int accountId,
        string query,
        CancellationToken cancellationToken)
    {
        var messages = await _messageRepository.FindAsync(
            m => m.AccountId == accountId
                && m.Content != null
                && m.Content.ToLower().Contains(query),
            cancellationToken);

        return messages.Select(m => new MessageSearchResult
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            Content = m.Content,
            SenderType = m.SenderType,
            MessageType = m.MessageType.ToString(),
            CreatedAt = m.CreatedAt
        }).ToList();
    }
}
