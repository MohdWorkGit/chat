using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Services.Conversations;

public class ConversationService : IConversationService
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public ConversationService(
        IRepository<Conversation> conversationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<ConversationDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id, cancellationToken);
        if (conversation is null)
            return null;

        return MapToDto(conversation);
    }

    public async Task<PaginatedResultDto<ConversationDto>> GetByAccountAsync(
        int accountId,
        ConversationFilterDto filter,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var spec = BuildSpecification(accountId, filter);
        var conversations = await _conversationRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _conversationRepository.CountAsync(spec, cancellationToken);

        var items = conversations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PaginatedResultDto<ConversationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ConversationDto> CreateAsync(
        int accountId,
        CreateConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        var conversation = new Conversation
        {
            AccountId = accountId,
            InboxId = request.InboxId,
            ContactId = request.ContactId,
            AssigneeId = request.AssigneeId,
            TeamId = request.TeamId,
            Status = (int)request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new ConversationCreatedEvent(conversation.Id, accountId), cancellationToken);

        return MapToDto(conversation);
    }

    public async Task UpdateStatusAsync(long conversationId, ConversationStatus status, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        conversation.Status = (int)status;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new ConversationUpdatedEvent(conversationId, conversation.AccountId, nameof(status)), cancellationToken);
    }

    public async Task AssignAsync(long conversationId, int? agentId, int? teamId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        conversation.AssigneeId = agentId;
        conversation.TeamId = teamId;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new ConversationAssignedEvent(conversationId, conversation.AccountId, agentId, teamId), cancellationToken);
    }

    public async Task TogglePriorityAsync(long conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        conversation.Priority = conversation.Priority == null || conversation.Priority == 0 ? 1 : 0;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MuteAsync(long conversationId, CancellationToken cancellationToken = default)
    {
        await UpdateStatusAsync(conversationId, ConversationStatus.Muted);
    }

    public async Task SnoozeAsync(long conversationId, DateTime snoozeUntil, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        conversation.Status = (int)ConversationStatus.Snoozed;
        conversation.SnoozedUntil = snoozeUntil;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new ConversationUpdatedEvent(conversationId, conversation.AccountId, "snoozed"), cancellationToken);
    }

    public async Task ResolveAsync(long conversationId, CancellationToken cancellationToken = default)
    {
        await UpdateStatusAsync(conversationId, ConversationStatus.Resolved);
    }

    public async Task ReopenAsync(long conversationId, CancellationToken cancellationToken = default)
    {
        await UpdateStatusAsync(conversationId, ConversationStatus.Open);
    }

    private static ConversationDto MapToDto(Conversation conversation)
    {
        return new ConversationDto
        {
            Id = conversation.Id,
            AccountId = conversation.AccountId,
            InboxId = conversation.InboxId,
            ContactId = conversation.ContactId,
            AssigneeId = conversation.AssigneeId,
            TeamId = conversation.TeamId,
            Status = conversation.Status,
            Priority = conversation.Priority,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }

    private static object BuildSpecification(int accountId, ConversationFilterDto filter)
    {
        // Returns a specification object used by the repository to filter conversations.
        // The actual specification pattern implementation resides in the infrastructure layer.
        return new { accountId, filter };
    }
}

// Domain Events
public record ConversationCreatedEvent(long ConversationId, int AccountId) : INotification;
public record ConversationUpdatedEvent(long ConversationId, int AccountId, string ChangedProperty) : INotification;
public record ConversationAssignedEvent(long ConversationId, int AccountId, int? AgentId, int? TeamId) : INotification;
