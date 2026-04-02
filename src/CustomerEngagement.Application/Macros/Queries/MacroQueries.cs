using System.Linq.Expressions;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Macros.Queries;

public record GetMacrosQuery(long AccountId, int Page, int PageSize) : IRequest<object>;

public record GetMacroByIdQuery(long AccountId, long Id) : IRequest<object>;

public class GetMacrosQueryHandler : IRequestHandler<GetMacrosQuery, object>
{
    private readonly IRepository<Macro> _repository;

    public GetMacrosQueryHandler(IRepository<Macro> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetMacrosQuery request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        Expression<Func<Macro, bool>> predicate = m => m.AccountId == accountId;

        var macros = await _repository.GetPagedAsync(
            request.Page, request.PageSize, predicate, m => m.CreatedAt, ascending: false, cancellationToken);

        var totalCount = await _repository.CountAsync(predicate, cancellationToken);

        return new
        {
            Data = macros.Select(m => new
            {
                m.Id,
                m.AccountId,
                m.Name,
                m.Actions,
                m.Visibility,
                m.CreatedById,
                m.CreatedAt,
                m.UpdatedAt
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

public class GetMacroByIdQueryHandler : IRequestHandler<GetMacroByIdQuery, object>
{
    private readonly IRepository<Macro> _repository;

    public GetMacroByIdQueryHandler(IRepository<Macro> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetMacroByIdQuery request, CancellationToken cancellationToken)
    {
        var macro = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (macro is null || macro.AccountId != (int)request.AccountId)
            return null!;

        return new
        {
            macro.Id,
            macro.AccountId,
            macro.Name,
            macro.Actions,
            macro.Visibility,
            macro.CreatedById,
            macro.CreatedAt,
            macro.UpdatedAt
        };
    }
}
