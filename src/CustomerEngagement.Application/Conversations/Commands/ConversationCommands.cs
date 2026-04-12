using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Enums;
using MediatR;

namespace CustomerEngagement.Application.Conversations.Commands;

public record CreateConversationCommand(
    long AccountId,
    int InboxId,
    int ContactId,
    string? Message,
    int? AssigneeId,
    int? TeamId,
    string? Status) : IRequest<long>;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, long>
{
    private readonly IConversationService _conversationService;

    public CreateConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task<long> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateConversationRequest
        {
            InboxId = request.InboxId,
            ContactId = request.ContactId,
            InitialMessage = request.Message,
            AssigneeId = request.AssigneeId,
            TeamId = request.TeamId
        };

        var result = await _conversationService.CreateAsync((int)request.AccountId, createRequest, cancellationToken);
        return result.Id;
    }
}

public record UpdateConversationCommand(
    long AccountId,
    long Id,
    string? Status,
    int? AssigneeId,
    int? TeamId,
    string? Priority,
    bool? Muted) : IRequest;

public class UpdateConversationCommandHandler : IRequestHandler<UpdateConversationCommand>
{
    private readonly IConversationService _conversationService;

    public UpdateConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(UpdateConversationCommand request, CancellationToken cancellationToken)
    {
        if (request.Status is not null && Enum.TryParse<ConversationStatus>(request.Status, true, out var status))
            await _conversationService.UpdateStatusAsync(request.Id, status, cancellationToken);

        if (request.AssigneeId.HasValue || request.TeamId.HasValue)
            await _conversationService.AssignAsync(request.Id, request.AssigneeId, request.TeamId, cancellationToken);

        if (request.Muted == true)
            await _conversationService.MuteAsync(request.Id, cancellationToken);
    }
}

public record ResolveConversationCommand(long AccountId, long ConversationId) : IRequest;

public class ResolveConversationCommandHandler : IRequestHandler<ResolveConversationCommand>
{
    private readonly IConversationService _conversationService;

    public ResolveConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(ResolveConversationCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.ResolveAsync(request.ConversationId, cancellationToken);
    }
}

public record ReopenConversationCommand(long AccountId, long ConversationId) : IRequest;

public class ReopenConversationCommandHandler : IRequestHandler<ReopenConversationCommand>
{
    private readonly IConversationService _conversationService;

    public ReopenConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(ReopenConversationCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.ReopenAsync(request.ConversationId, cancellationToken);
    }
}

public record MuteConversationCommand(long AccountId, long ConversationId) : IRequest;

public class MuteConversationCommandHandler : IRequestHandler<MuteConversationCommand>
{
    private readonly IConversationService _conversationService;

    public MuteConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(MuteConversationCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.MuteAsync(request.ConversationId, cancellationToken);
    }
}

public record UnmuteConversationCommand(long AccountId, long ConversationId) : IRequest;

public class UnmuteConversationCommandHandler : IRequestHandler<UnmuteConversationCommand>
{
    private readonly IConversationService _conversationService;

    public UnmuteConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(UnmuteConversationCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.UnmuteAsync(request.ConversationId, cancellationToken);
    }
}

public record SnoozeConversationCommand(
    long AccountId,
    long ConversationId,
    DateTime SnoozedUntil) : IRequest;

public class SnoozeConversationCommandHandler : IRequestHandler<SnoozeConversationCommand>
{
    private readonly IConversationService _conversationService;

    public SnoozeConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(SnoozeConversationCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.SnoozeAsync(request.ConversationId, request.SnoozedUntil, cancellationToken);
    }
}

public record TogglePriorityCommand(long AccountId, long ConversationId) : IRequest;

public class TogglePriorityCommandHandler : IRequestHandler<TogglePriorityCommand>
{
    private readonly IConversationService _conversationService;

    public TogglePriorityCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(TogglePriorityCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.TogglePriorityAsync(request.ConversationId, cancellationToken);
    }
}

public record AssignConversationCommand(
    long AccountId,
    long ConversationId,
    int? AssigneeId,
    int? TeamId) : IRequest;

public class AssignConversationCommandHandler : IRequestHandler<AssignConversationCommand>
{
    private readonly IConversationService _conversationService;

    public AssignConversationCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(AssignConversationCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.AssignAsync(request.ConversationId, request.AssigneeId, request.TeamId, cancellationToken);
    }
}

public record AddParticipantCommand(long AccountId, long ConversationId, long UserId) : IRequest;

public class AddParticipantCommandHandler : IRequestHandler<AddParticipantCommand>
{
    private readonly IConversationService _conversationService;

    public AddParticipantCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(AddParticipantCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.AddParticipantAsync(
            request.ConversationId, request.UserId, (int)request.AccountId, cancellationToken);
    }
}

public record RemoveParticipantCommand(long AccountId, long ConversationId, long UserId) : IRequest;

public class RemoveParticipantCommandHandler : IRequestHandler<RemoveParticipantCommand>
{
    private readonly IConversationService _conversationService;

    public RemoveParticipantCommandHandler(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        await _conversationService.RemoveParticipantAsync(
            request.ConversationId, request.UserId, (int)request.AccountId, cancellationToken);
    }
}
