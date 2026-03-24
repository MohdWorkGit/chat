using MediatR;

namespace CustomerEngagement.Application.AutomationRules.Queries;

public record GetAutomationRulesQuery(long AccountId, int Page, int PageSize) : IRequest<object>;

public record GetAutomationRuleByIdQuery(long AccountId, long Id) : IRequest<object>;
