using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Automations;

public class AutomationRuleEngine : IAutomationRuleEngine
{
    private readonly IRepository<AutomationRule> _ruleRepository;
    private readonly IConversationService _conversationService;
    private readonly IAssignmentService _assignmentService;
    private readonly ILogger<AutomationRuleEngine> _logger;

    public AutomationRuleEngine(
        IRepository<AutomationRule> ruleRepository,
        IConversationService conversationService,
        IAssignmentService assignmentService,
        ILogger<AutomationRuleEngine> logger)
    {
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EvaluateAsync(string eventName, AutomationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Load all active rules for this account and event
            var rules = await _ruleRepository.ListAsync(
                new { AccountId = context.AccountId, EventName = eventName, IsActive = true },
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
        switch (action.ActionType.ToLowerInvariant())
        {
            case "assign_agent":
                if (context.ConversationId.HasValue && action.Parameters.TryGetValue("agent_id", out var agentIdObj))
                {
                    var agentId = Convert.ToInt32(agentIdObj);
                    await _assignmentService.AssignToAgentAsync(context.ConversationId.Value, agentId, cancellationToken);
                }
                break;

            case "assign_team":
                if (context.ConversationId.HasValue && action.Parameters.TryGetValue("team_id", out var teamIdObj))
                {
                    var teamId = Convert.ToInt32(teamIdObj);
                    await _assignmentService.AssignToTeamAsync(context.ConversationId.Value, teamId, cancellationToken);
                }
                break;

            case "update_status":
                if (context.ConversationId.HasValue && action.Parameters.TryGetValue("status", out var statusObj))
                {
                    var status = Enum.Parse<ConversationStatus>(statusObj.ToString()!, true);
                    await _conversationService.UpdateStatusAsync(context.ConversationId.Value, status, cancellationToken);
                }
                break;

            case "resolve":
                if (context.ConversationId.HasValue)
                {
                    await _conversationService.ResolveAsync(context.ConversationId.Value, cancellationToken);
                }
                break;

            case "mute":
                if (context.ConversationId.HasValue)
                {
                    await _conversationService.MuteAsync(context.ConversationId.Value, cancellationToken);
                }
                break;

            default:
                _logger.LogWarning("Unknown automation action type: {ActionType}", action.ActionType);
                break;
        }
    }

    private static AutomationRuleDto MapToDto(AutomationRule rule)
    {
        return new AutomationRuleDto
        {
            Id = rule.Id,
            AccountId = rule.AccountId,
            Name = rule.Name,
            Description = rule.Description,
            EventName = rule.EventName,
            ConditionOperator = "AND",
            IsActive = rule.Active
            // Conditions and Actions would be deserialized from JSON stored in the entity
        };
    }
}
