using System.Text.Json;
using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Application.Services.Integrations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Automations;

public class AutomationRuleEngine : IAutomationRuleEngine
{
    private readonly IRepository<AutomationRule> _ruleRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Label> _labelRepository;
    private readonly IRepository<Team> _teamRepository;
    private readonly IRepository<TeamMember> _teamMemberRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IConversationService _conversationService;
    private readonly IAssignmentService _assignmentService;
    private readonly IMessageService _messageService;
    private readonly IWebhookService _webhookService;
    private readonly IEmailSender _emailSender;
    private readonly IBackgroundJobClient _jobClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<AutomationRuleEngine> _logger;

    public AutomationRuleEngine(
        IRepository<AutomationRule> ruleRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Label> labelRepository,
        IRepository<Team> teamRepository,
        IRepository<TeamMember> teamMemberRepository,
        IRepository<User> userRepository,
        IConversationService conversationService,
        IAssignmentService assignmentService,
        IMessageService messageService,
        IWebhookService webhookService,
        IEmailSender emailSender,
        IBackgroundJobClient jobClient,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<AutomationRuleEngine> logger)
    {
        _ruleRepository = ruleRepository;
        _conversationRepository = conversationRepository;
        _labelRepository = labelRepository;
        _teamRepository = teamRepository;
        _teamMemberRepository = teamMemberRepository;
        _userRepository = userRepository;
        _conversationService = conversationService;
        _assignmentService = assignmentService;
        _messageService = messageService;
        _webhookService = webhookService;
        _emailSender = emailSender;
        _jobClient = jobClient;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task EvaluateAsync(string eventName, AutomationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var rules = await _ruleRepository.FindAsync(
                r => r.AccountId == context.AccountId
                     && r.EventName == eventName
                     && r.Active,
                cancellationToken);

            foreach (var rule in rules)
            {
                var ruleDto = MapToDto(rule);

                if (EvaluateConditions(ruleDto, context))
                {
                    _logger.LogInformation(
                        "Automation rule {RuleId} matched for event {EventName} in account {AccountId}",
                        rule.Id, eventName, context.AccountId);

                    await ExecuteActionsAsync(ruleDto, context, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating automation rules for event {EventName} in account {AccountId}",
                eventName, context.AccountId);
        }
    }

    public async Task ExecuteActionsAsync(AutomationRuleDto rule, AutomationContext context, CancellationToken cancellationToken = default)
    {
        foreach (var action in rule.Actions)
        {
            try
            {
                await ExecuteActionAsync(action, context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing automation action {ActionType} for rule {RuleId}",
                    action.ActionType, rule.Id);
            }
        }
    }

    private bool EvaluateConditions(AutomationRuleDto rule, AutomationContext context)
    {
        if (rule.Conditions.Count == 0)
            return true;

        var results = rule.Conditions.Select(c => EvaluateCondition(c, context));

        return rule.ConditionOperator.Equals("OR", StringComparison.OrdinalIgnoreCase)
            ? results.Any(r => r)
            : results.All(r => r);
    }

    private static bool EvaluateCondition(AutomationConditionDto condition, AutomationContext context)
    {
        if (!context.Properties.TryGetValue(condition.Attribute, out var actualValue))
            return false;

        var actualStr = actualValue?.ToString() ?? string.Empty;
        var expectedStr = condition.Value;

        return condition.Operator.ToLowerInvariant() switch
        {
            "equals" => string.Equals(actualStr, expectedStr, StringComparison.OrdinalIgnoreCase),
            "not_equals" => !string.Equals(actualStr, expectedStr, StringComparison.OrdinalIgnoreCase),
            "contains" => actualStr.Contains(expectedStr, StringComparison.OrdinalIgnoreCase),
            "not_contains" => !actualStr.Contains(expectedStr, StringComparison.OrdinalIgnoreCase),
            "starts_with" => actualStr.StartsWith(expectedStr, StringComparison.OrdinalIgnoreCase),
            "ends_with" => actualStr.EndsWith(expectedStr, StringComparison.OrdinalIgnoreCase),
            "is_present" => !string.IsNullOrEmpty(actualStr),
            "is_not_present" => string.IsNullOrEmpty(actualStr),
            _ => false
        };
    }

    private async Task ExecuteActionAsync(AutomationActionDto action, AutomationContext context, CancellationToken cancellationToken)
    {
        var conversationId = context.ConversationId;

        switch (action.ActionType.ToLowerInvariant())
        {
            case "assign_agent":
                if (conversationId.HasValue && TryGetInt(action, "agent_id", out var agentId))
                    await _assignmentService.AssignToAgentAsync(conversationId.Value, agentId, cancellationToken);
                break;

            case "assign_team":
                if (conversationId.HasValue && TryGetInt(action, "team_id", out var teamId))
                    await _assignmentService.AssignToTeamAsync(conversationId.Value, teamId, cancellationToken);
                break;

            case "update_status":
            case "change_status":
                if (conversationId.HasValue && action.Parameters.TryGetValue("status", out var statusObj))
                {
                    var status = Enum.Parse<ConversationStatus>(statusObj.ToString()!, true);
                    await _conversationService.UpdateStatusAsync(conversationId.Value, status, cancellationToken);
                }
                break;

            case "resolve":
                if (conversationId.HasValue)
                    await _conversationService.ResolveAsync(conversationId.Value, cancellationToken);
                break;

            case "open_conversation":
                if (conversationId.HasValue)
                    await _conversationService.UpdateStatusAsync(conversationId.Value, ConversationStatus.Open, cancellationToken);
                break;

            case "mute":
            case "mute_conversation":
                if (conversationId.HasValue)
                    await _conversationService.MuteAsync(conversationId.Value, cancellationToken);
                break;

            case "snooze_conversation":
                if (conversationId.HasValue)
                {
                    var until = action.Parameters.TryGetValue("snooze_until", out var untilObj)
                        ? DateTime.Parse(untilObj.ToString()!).ToUniversalTime()
                        : DateTime.UtcNow.AddHours(1);
                    await _conversationService.SnoozeAsync(conversationId.Value, until, cancellationToken);
                }
                break;

            case "change_priority":
                if (conversationId.HasValue)
                    await _conversationService.TogglePriorityAsync(conversationId.Value, cancellationToken);
                break;

            case "send_message":
                if (conversationId.HasValue && action.Parameters.TryGetValue("message", out var msgObj))
                {
                    await _messageService.CreateAsync(
                        conversationId.Value,
                        new CreateMessageRequest
                        {
                            Content = msgObj.ToString() ?? string.Empty,
                            MessageType = (int)MessageType.Outgoing,
                            ContentType = "text"
                        },
                        cancellationToken);
                }
                break;

            case "add_private_note":
                if (conversationId.HasValue && action.Parameters.TryGetValue("note", out var noteObj))
                {
                    await _messageService.CreateAsync(
                        conversationId.Value,
                        new CreateMessageRequest
                        {
                            Content = noteObj.ToString() ?? string.Empty,
                            MessageType = (int)MessageType.Outgoing,
                            ContentType = "text",
                            IsPrivate = true
                        },
                        cancellationToken);
                }
                break;

            case "add_label":
                if (conversationId.HasValue && TryGetLabelNames(action, out var addNames))
                    await ApplyLabelsAsync(conversationId.Value, context.AccountId, addNames, add: true, cancellationToken);
                break;

            case "remove_label":
                if (conversationId.HasValue && TryGetLabelNames(action, out var removeNames))
                    await ApplyLabelsAsync(conversationId.Value, context.AccountId, removeNames, add: false, cancellationToken);
                break;

            case "send_email_to_team":
                if (TryGetInt(action, "team_id", out var emailTeamId)
                    && action.Parameters.TryGetValue("subject", out var subjObj)
                    && action.Parameters.TryGetValue("body", out var bodyObj))
                {
                    await SendEmailToTeamAsync(emailTeamId, subjObj.ToString()!, bodyObj.ToString()!, cancellationToken);
                }
                break;

            case "send_webhook_event":
                {
                    var eventType = action.Parameters.TryGetValue("event", out var eventObj)
                        ? eventObj.ToString() ?? "automation_rule"
                        : "automation_rule";
                    var payload = action.Parameters.TryGetValue("payload", out var payloadObj)
                        ? payloadObj
                        : new { context.AccountId, context.ConversationId };
                    await _webhookService.FireWebhookAsync(context.AccountId, eventType, payload, cancellationToken);
                }
                break;

            case "send_email_transcript":
                if (conversationId.HasValue && action.Parameters.TryGetValue("email", out var emailObj))
                {
                    var recipient = emailObj.ToString()!;
                    var convoId = (int)conversationId.Value;
                    _jobClient.Enqueue<EmailTranscriptJob>(job =>
                        job.ExecuteAsync(convoId, recipient, CancellationToken.None));
                }
                break;

            default:
                _logger.LogWarning("Unknown automation action type: {ActionType}", action.ActionType);
                break;
        }
    }

    private async Task ApplyLabelsAsync(long conversationId, int accountId, IReadOnlyList<string> labelNames,
        bool add, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync((int)conversationId, cancellationToken);
        if (conversation is null)
            return;

        var labels = await _labelRepository.FindAsync(
            l => l.AccountId == accountId && labelNames.Contains(l.Title),
            cancellationToken);

        foreach (var label in labels)
        {
            if (add)
            {
                if (!conversation.Labels.Any(l => l.Id == label.Id))
                    conversation.Labels.Add(label);
            }
            else
            {
                var existing = conversation.Labels.FirstOrDefault(l => l.Id == label.Id);
                if (existing is not null)
                    conversation.Labels.Remove(existing);
            }
        }

        conversation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SendEmailToTeamAsync(int teamId, string subject, string body, CancellationToken cancellationToken)
    {
        var members = await _teamMemberRepository.FindAsync(m => m.TeamId == teamId, cancellationToken);
        if (members.Count == 0)
            return;

        var userIds = members.Select(m => m.UserId).ToList();
        var users = await _userRepository.FindAsync(u => userIds.Contains(u.Id), cancellationToken);

        foreach (var user in users)
        {
            if (string.IsNullOrEmpty(user.Email))
                continue;

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    user.Name ?? user.Email,
                    subject,
                    body,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send team email to {Email}", user.Email);
            }
        }
    }

    private static bool TryGetInt(AutomationActionDto action, string key, out int value)
    {
        value = 0;
        if (!action.Parameters.TryGetValue(key, out var obj) || obj is null)
            return false;
        return int.TryParse(obj.ToString(), out value);
    }

    private static bool TryGetLabelNames(AutomationActionDto action, out IReadOnlyList<string> names)
    {
        names = Array.Empty<string>();
        if (!action.Parameters.TryGetValue("labels", out var obj) || obj is null)
            return false;

        var result = new List<string>();
        if (obj is IEnumerable<object> enumerable)
        {
            result.AddRange(enumerable.Select(o => o.ToString() ?? string.Empty).Where(s => s.Length > 0));
        }
        else if (obj is JsonElement element && element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                result.Add(item.GetString() ?? string.Empty);
        }
        else
        {
            result.Add(obj.ToString() ?? string.Empty);
        }

        names = result.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        return names.Count > 0;
    }

    private static AutomationRuleDto MapToDto(AutomationRule rule)
    {
        var conditions = rule.GetConditions()
            .Select(c => new AutomationConditionDto
            {
                Attribute = c.AttributeKey,
                Operator = c.FilterOperator,
                Value = c.Values.FirstOrDefault() ?? string.Empty
            })
            .ToList();

        var actions = rule.GetActions()
            .Select(a => new AutomationActionDto
            {
                ActionType = a.ActionName,
                Parameters = ParseActionParameters(a.ActionParams)
            })
            .ToList();

        return new AutomationRuleDto
        {
            Id = rule.Id,
            AccountId = rule.AccountId,
            Name = rule.Name,
            Description = rule.Description ?? string.Empty,
            EventName = rule.EventName,
            ConditionOperator = "AND",
            IsActive = rule.Active,
            Conditions = conditions,
            Actions = actions
        };
    }

    private static Dictionary<string, object> ParseActionParameters(List<string> actionParams)
    {
        if (actionParams.Count == 0)
            return new Dictionary<string, object>();

        // Single JSON-encoded parameter bag: [{"key":"value"}]
        if (actionParams.Count == 1)
        {
            var single = actionParams[0];
            if (!string.IsNullOrWhiteSpace(single) && single.TrimStart().StartsWith('{'))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, object>>(single);
                    if (parsed is not null)
                        return parsed;
                }
                catch (JsonException) { /* fall through */ }
            }
        }

        // Fallback: pass values indexed by position and aggregated under "values"
        return new Dictionary<string, object> { ["values"] = actionParams };
    }
}
