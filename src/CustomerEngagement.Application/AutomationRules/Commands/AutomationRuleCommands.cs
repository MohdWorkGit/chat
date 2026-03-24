using MediatR;

namespace CustomerEngagement.Application.AutomationRules.Commands;

public record CreateAutomationRuleCommand(long AccountId = 0, string Name = "", string? EventName = null) : IRequest<object>;

public record UpdateAutomationRuleCommand(long AccountId = 0, long Id = 0, string? Name = null, string? EventName = null) : IRequest<object>;

public record DeleteAutomationRuleCommand(long AccountId, long Id) : IRequest<object>;
