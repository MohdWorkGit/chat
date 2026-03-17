using System.Text.Json;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Conversations;

public interface IFilterService
{
    Task<FilteredConversationResult> ApplyFilterAsync(int accountId, ConversationFilterRequest filter, CancellationToken cancellationToken = default);
    Task<List<SavedFilterDto>> GetSavedFiltersAsync(int accountId, int userId, CancellationToken cancellationToken = default);
    Task<SavedFilterDto> CreateFilterAsync(int accountId, int userId, CreateFilterRequest request, CancellationToken cancellationToken = default);
    Task DeleteFilterAsync(int filterId, CancellationToken cancellationToken = default);
}

public class FilterService : IFilterService
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<CustomFilter> _customFilterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FilterService> _logger;

    public FilterService(
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IRepository<CustomFilter> customFilterRepository,
        IUnitOfWork unitOfWork,
        ILogger<FilterService> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _customFilterRepository = customFilterRepository ?? throw new ArgumentNullException(nameof(customFilterRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FilteredConversationResult> ApplyFilterAsync(
        int accountId,
        ConversationFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.FindAsync(
            c => c.AccountId == accountId,
            cancellationToken);

        var filtered = conversations.AsEnumerable();

        if (filter.Status.HasValue)
        {
            filtered = filtered.Where(c => c.Status == (CustomerEngagement.Core.Enums.ConversationStatus)(int)filter.Status.Value);
        }

        if (filter.InboxId.HasValue)
        {
            filtered = filtered.Where(c => c.InboxId == filter.InboxId.Value);
        }

        if (filter.AssigneeId.HasValue)
        {
            filtered = filtered.Where(c => c.AssigneeId == filter.AssigneeId.Value);
        }

        if (filter.TeamId.HasValue)
        {
            filtered = filtered.Where(c => c.TeamId == filter.TeamId.Value);
        }

        if (filter.LabelId.HasValue)
        {
            filtered = filtered.Where(c =>
                c.Labels.Any(l => l.Id == filter.LabelId.Value));
        }

        if (filter.Priority.HasValue)
        {
            filtered = filtered.Where(c => c.Priority == filter.Priority.Value);
        }

        if (filter.CreatedAfter.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt >= filter.CreatedAfter.Value);
        }

        if (filter.CreatedBefore.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt <= filter.CreatedBefore.Value);
        }

        if (filter.Unattended == true)
        {
            var unattendedIds = await GetUnattendedConversationIdsAsync(accountId, cancellationToken);
            filtered = filtered.Where(c => unattendedIds.Contains(c.Id));
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;
        var page = filter.Page > 0 ? filter.Page : 1;
        var pageSize = filter.PageSize > 0 ? filter.PageSize : 25;

        var items = filteredList
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToFilteredDto)
            .ToList();

        return new FilteredConversationResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<SavedFilterDto>> GetSavedFiltersAsync(
        int accountId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var filters = await _customFilterRepository.FindAsync(
            f => f.AccountId == accountId && f.UserId == userId && f.FilterType == "conversation",
            cancellationToken);

        return filters.Select(f => new SavedFilterDto
        {
            Id = f.Id,
            Name = f.Name,
            FilterType = f.FilterType,
            Query = f.Query,
            CreatedAt = f.CreatedAt
        }).ToList();
    }

    public async Task<SavedFilterDto> CreateFilterAsync(
        int accountId,
        int userId,
        CreateFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var customFilter = new CustomFilter
        {
            AccountId = accountId,
            UserId = userId,
            Name = request.Name,
            FilterType = "conversation",
            Query = JsonSerializer.Serialize(request.Filter),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _customFilterRepository.AddAsync(customFilter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SavedFilterDto
        {
            Id = customFilter.Id,
            Name = customFilter.Name,
            FilterType = customFilter.FilterType,
            Query = customFilter.Query,
            CreatedAt = customFilter.CreatedAt
        };
    }

    public async Task DeleteFilterAsync(int filterId, CancellationToken cancellationToken = default)
    {
        var filter = await _customFilterRepository.GetByIdAsync(filterId, cancellationToken)
            ?? throw new InvalidOperationException($"Filter {filterId} not found.");

        await _customFilterRepository.DeleteAsync(filter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<HashSet<int>> GetUnattendedConversationIdsAsync(
        int accountId,
        CancellationToken cancellationToken)
    {
        // An unattended conversation is one where the last message is incoming (no agent reply yet)
        var messages = await _messageRepository.FindAsync(
            m => m.AccountId == accountId,
            cancellationToken);

        var unattendedIds = messages
            .GroupBy(m => m.ConversationId)
            .Where(g =>
            {
                var lastMessage = g.OrderByDescending(m => m.CreatedAt).First();
                return lastMessage.MessageType == MessageType.Incoming;
            })
            .Select(g => g.Key)
            .ToHashSet();

        return unattendedIds;
    }

    private static FilteredConversationDto MapToFilteredDto(Conversation conversation)
    {
        return new FilteredConversationDto
        {
            Id = conversation.Id,
            AccountId = conversation.AccountId,
            InboxId = conversation.InboxId,
            ContactId = conversation.ContactId,
            AssigneeId = conversation.AssigneeId,
            TeamId = conversation.TeamId,
            Status = (ConversationStatus)(int)conversation.Status,
            Priority = (ConversationPriority)(int)conversation.Priority,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }
}

public class ConversationFilterRequest
{
    public ConversationStatus? Status { get; set; }
    public int? InboxId { get; set; }
    public int? AssigneeId { get; set; }
    public int? TeamId { get; set; }
    public int? LabelId { get; set; }
    public ConversationPriority? Priority { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public bool? Unattended { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class CreateFilterRequest
{
    public required string Name { get; set; }
    public ConversationFilterRequest Filter { get; set; } = new();
}

public class FilteredConversationDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int InboxId { get; set; }
    public int ContactId { get; set; }
    public int? AssigneeId { get; set; }
    public int? TeamId { get; set; }
    public ConversationStatus Status { get; set; }
    public ConversationPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FilteredConversationResult
{
    public List<FilteredConversationDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class SavedFilterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? FilterType { get; set; }
    public string? Query { get; set; }
    public DateTime CreatedAt { get; set; }
}
