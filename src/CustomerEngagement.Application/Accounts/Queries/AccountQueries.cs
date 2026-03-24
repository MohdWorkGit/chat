using MediatR;

namespace CustomerEngagement.Application.Accounts.Queries;

public record GetAccountsQuery(int Page, int PageSize) : IRequest<object>;

public record GetAccountByIdQuery(long Id) : IRequest<object>;
