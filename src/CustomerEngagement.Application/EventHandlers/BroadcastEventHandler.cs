using CustomerEngagement.Application.Hubs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class BroadcastEventHandler :
    INotificationHandler<ConversationCreatedEvent>,
    INotificationHandler<MessageCreatedEvent>,
    INotificationHandler<ConversationStatusChangedEvent>
{
    private readonly IHubContext<ConversationHub> _hubContext;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly ILogger<BroadcastEventHandler> _logger;

    public BroadcastEventHandler(
        IHubContext<ConversationHub> hubContext,
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        ILogger<BroadcastEventHandler> logger)
    {
        _hubContext = hubContext;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task Handle(ConversationCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Broadcasting conversation.created for Conversation {ConversationId} in Account {AccountId}",
            notification.ConversationId, notification.AccountId);

        var conversation = await _conversationRepository.GetByIdAsync(notification.ConversationId, cancellationToken);
        if (conversation is null)
            return;

        var payload = new
        {
            conversation.Id,
            conversation.AccountId,
            conversation.InboxId,
            conversation.ContactId,
            conversation.AssigneeId,
            conversation.TeamId,
            conversation.Status,
            conversation.CreatedAt
        };

        await Task.WhenAll(
            _hubContext.Clients.Group($"account_{notification.AccountId}")
                .SendAsync("conversation.created", payload, cancellationToken),
            _hubContext.Clients.Group($"conversation_{notification.ConversationId}")
                .SendAsync("conversation.created", payload, cancellationToken));
    }

    public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Broadcasting message.created for Message {MessageId} in Conversation {ConversationId}",
            notification.MessageId, notification.ConversationId);

        var message = await _messageRepository.GetByIdAsync(notification.MessageId, cancellationToken);
        if (message is null)
            return;

        var payload = new
        {
            message.Id,
            message.ConversationId,
            message.AccountId,
            message.Content,
            message.MessageType,
            message.SenderId,
            message.SenderType,
            message.CreatedAt
        };

        await Task.WhenAll(
            _hubContext.Clients.Group($"account_{notification.AccountId}")
                .SendAsync("message.created", payload, cancellationToken),
            _hubContext.Clients.Group($"conversation_{notification.ConversationId}")
                .SendAsync("message.created", payload, cancellationToken));
    }

    public async Task Handle(ConversationStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Broadcasting conversation.status_changed for Conversation {ConversationId}: {PreviousStatus} -> {NewStatus}",
            notification.ConversationId, notification.PreviousStatus, notification.NewStatus);

        var payload = new
        {
            ConversationId = notification.ConversationId,
            AccountId = notification.AccountId,
            PreviousStatus = notification.PreviousStatus,
            NewStatus = notification.NewStatus
        };

        await Task.WhenAll(
            _hubContext.Clients.Group($"account_{notification.AccountId}")
                .SendAsync("conversation.status_changed", payload, cancellationToken),
            _hubContext.Clients.Group($"conversation_{notification.ConversationId}")
                .SendAsync("conversation.status_changed", payload, cancellationToken));
    }
}
