using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Contacts;

public interface IContactSearchService
{
    Task<PaginatedContactSearchResult> SearchAsync(int accountId, string query, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
    Task<PaginatedContactSearchResult> FilterAsync(int accountId, ContactFilterRequest filter, CancellationToken cancellationToken = default);
    Task<List<ContactSummaryDto>> GetRecentAsync(int accountId, int count = 10, CancellationToken cancellationToken = default);
}

public class ContactSearchService : IContactSearchService
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly ILogger<ContactSearchService> _logger;

    public ContactSearchService(
        IRepository<Contact> contactRepository,
        ILogger<ContactSearchService> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaginatedContactSearchResult> SearchAsync(
        int accountId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return PaginatedContactSearchResult.Empty(page, pageSize);
        }

        var normalizedQuery = query.Trim().ToLowerInvariant();

        var contacts = await _contactRepository.FindAsync(
            c => c.AccountId == accountId
                && (c.Name != null && c.Name.ToLower().Contains(normalizedQuery)
                    || c.Email != null && c.Email.ToLower().Contains(normalizedQuery)
                    || c.Phone != null && c.Phone.ToLower().Contains(normalizedQuery)
                    || c.Identifier != null && c.Identifier.ToLower().Contains(normalizedQuery)),
            cancellationToken);

        var contactList = contacts.ToList();
        var totalCount = contactList.Count;

        var items = contactList
            .OrderByDescending(c => c.LastActivityAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToSummaryDto)
            .ToList();

        return new PaginatedContactSearchResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedContactSearchResult> FilterAsync(
        int accountId,
        ContactFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var contacts = await _contactRepository.FindAsync(
            c => c.AccountId == accountId,
            cancellationToken);

        var filtered = contacts.AsEnumerable();

        if (filter.ContactType.HasValue)
        {
            filtered = filtered.Where(c => c.ContactType == filter.ContactType.Value);
        }

        if (filter.CreatedAfter.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt >= filter.CreatedAfter.Value);
        }

        if (filter.CreatedBefore.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt <= filter.CreatedBefore.Value);
        }

        if (filter.LabelIds is { Count: > 0 })
        {
            filtered = filtered.Where(c =>
                c.Labels.Any(l => filter.LabelIds.Contains(l.Id)));
        }

        if (filter.CustomAttributes is { Count: > 0 })
        {
            foreach (var (key, value) in filter.CustomAttributes)
            {
                filtered = filtered.Where(c =>
                    c.CustomAttributes != null
                    && c.CustomAttributes.Contains(key)
                    && c.CustomAttributes.Contains(value));
            }
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;
        var page = filter.Page > 0 ? filter.Page : 1;
        var pageSize = filter.PageSize > 0 ? filter.PageSize : 25;

        var items = filteredList
            .OrderByDescending(c => c.LastActivityAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToSummaryDto)
            .ToList();

        return new PaginatedContactSearchResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<ContactSummaryDto>> GetRecentAsync(
        int accountId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var contacts = await _contactRepository.FindAsync(
            c => c.AccountId == accountId,
            cancellationToken);

        return contacts
            .OrderByDescending(c => c.LastActivityAt ?? c.CreatedAt)
            .Take(count)
            .Select(MapToSummaryDto)
            .ToList();
    }

    private static ContactSummaryDto MapToSummaryDto(Contact contact)
    {
        return new ContactSummaryDto
        {
            Id = contact.Id,
            AccountId = contact.AccountId,
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            Identifier = contact.Identifier,
            ContactType = contact.ContactType,
            LastActivityAt = contact.LastActivityAt,
            CreatedAt = contact.CreatedAt
        };
    }
}

public class ContactFilterRequest
{
    public ContactType? ContactType { get; set; }
    public List<int>? LabelIds { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public Dictionary<string, string>? CustomAttributes { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class ContactSummaryDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Identifier { get; set; }
    public ContactType ContactType { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaginatedContactSearchResult
{
    public List<ContactSummaryDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public static PaginatedContactSearchResult Empty(int page, int pageSize) => new()
    {
        Items = [],
        TotalCount = 0,
        Page = page,
        PageSize = pageSize
    };
}
