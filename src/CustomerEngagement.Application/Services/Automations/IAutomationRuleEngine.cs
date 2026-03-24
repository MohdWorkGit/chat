using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Automations;

public interface IAutomationRuleEngine
{
    Task EvaluateAsync(string eventName, AutomationContext context, CancellationToken cancellationToken = default);

    Task ExecuteActionsAsync(AutomationRuleDto rule, AutomationContext context, CancellationToken cancellationToken = default);
}
