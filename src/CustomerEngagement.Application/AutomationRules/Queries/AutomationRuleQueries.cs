using System.Linq.Expressions;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.AutomationRules.Queries;

public record GetAutomationRulesQuery(long AccountId, int Page, int PageSize) : IRequest<object>;

public record GetAutomationRuleByIdQuery(long AccountId, long Id) : IRequest<object>;

public class GetAutomationRulesQueryHandler : IRequestHandler<GetAutomationRulesQuery, object>
{
    private readonly IRepository<AutomationRule> _repository;

    public GetAutomationRulesQueryHandler(IRepository<AutomationRule> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAutomationRulesQuery request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        Expression<Func<AutomationRule, bool>> predicate = r => r.AccountId == accountId;

        var rules = await _repository.GetPagedAsync(
            request.Page, request.PageSize, predicate, r => r.CreatedAt, ascending: false, cancellationToken);

        var totalCount = await _repository.CountAsync(predicate, cancellationToken);

        return new
        {
            Data = rules.Select(r => new
            {
                r.Id,
                r.AccountId,
                r.Name,
                r.Description,
                r.EventName,
                r.Conditions,
                r.Actions,
                r.Active,
                r.CreatedAt,
                r.UpdatedAt
            }),
            Meta = new
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        };
    }
}

public class GetAutomationRuleByIdQueryHandler : IRequestHandler<GetAutomationRuleByIdQuery, object>
{
    private readonly IRepository<AutomationRule> _repository;

    public GetAutomationRuleByIdQueryHandler(IRepository<AutomationRule> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAutomationRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (rule is null || rule.AccountId != (int)request.AccountId)
            return null!;

        return new
        {
            rule.Id,
            rule.AccountId,
            rule.Name,
            rule.Description,
            rule.EventName,
            rule.Conditions,
            rule.Actions,
            rule.Active,
            rule.CreatedAt,
            rule.UpdatedAt
        };
    }
}
