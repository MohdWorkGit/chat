using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Accounts.Queries;

public record GetAccountsQuery(int Page, int PageSize) : IRequest<object>;

public record GetAccountByIdQuery(long Id) : IRequest<object>;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, object>
{
    private readonly IRepository<Account> _accountRepository;

    public GetAccountsQueryHandler(IRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<object> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            cancellationToken: cancellationToken);

        var totalCount = await _accountRepository.CountAsync(cancellationToken: cancellationToken);

        return new
        {
            Data = accounts.Select(a => new
            {
                a.Id,
                a.Name,
                a.Locale,
                a.Domain,
                a.CreatedAt
            }).ToList(),
            Meta = new
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        };
    }
}

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, object>
{
    private readonly IRepository<Account> _accountRepository;

    public GetAccountByIdQueryHandler(IRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<object> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync((int)request.Id, cancellationToken);

        if (account is null)
            return new { Error = "Account not found" };

        return new
        {
            account.Id,
            account.Name,
            account.Locale,
            account.Domain,
            account.CreatedAt
        };
    }
}
