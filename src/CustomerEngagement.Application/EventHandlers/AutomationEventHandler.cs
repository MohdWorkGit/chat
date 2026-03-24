using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Automations;
using CustomerEngagement.Core.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class AutomationEventHandler :
    INotificationHandler<ConversationCreatedEvent>,
    INotificationHandler<MessageCreatedEvent>,
    INotificationHandler<ConversationStatusChangedEvent>
{
    private readonly IAutomationRuleEngine _automationRuleEngine;
    private readonly ILogger<AutomationEventHandler> _logger;

    public AutomationEventHandler(
        IAutomationRuleEngine automationRuleEngine,
        ILogger<AutomationEventHandler> logger)
    {
        _automationRuleEngine = automationRuleEngine;
        _logger = logger;
    }

    public async Task Handle(ConversationCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Evaluating automation rules for conversation_created, Conversation {ConversationId}",
                notification.ConversationId);

            var context = new AutomationContext
            {
                AccountId = notification.AccountId,
                ConversationId = notification.ConversationId,
                EventName = "conversation_created"
            };

            await _automationRuleEngine.EvaluateAsync("conversation_created", context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating automation rules for conversation_created, Conversation {ConversationId}",
                notification.ConversationId);
        }
    }

    public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Evaluating automation rules for message_created, Message {MessageId}",
                notification.MessageId);

            var context = new AutomationContext
            {
                AccountId = notification.AccountId,
                ConversationId = notification.ConversationId,
                MessageId = notification.MessageId,
                EventName = "message_created"
            };

            await _automationRuleEngine.EvaluateAsync("message_created", context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating automation rules for message_created, Message {MessageId}",
                notification.MessageId);
        }
    }

    public async Task Handle(ConversationStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Evaluating automation rules for conversation_status_changed, Conversation {ConversationId}",
                notification.ConversationId);

            var context = new AutomationContext
            {
                AccountId = notification.AccountId,
                ConversationId = notification.ConversationId,
                EventName = "conversation_status_changed",
                Properties = new Dictionary<string, object>
                {
                    ["previous_status"] = notification.PreviousStatus,
                    ["new_status"] = notification.NewStatus
                }
            };

            await _automationRuleEngine.EvaluateAsync("conversation_status_changed", context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating automation rules for conversation_status_changed, Conversation {ConversationId}",
                notification.ConversationId);
        }
    }
}
