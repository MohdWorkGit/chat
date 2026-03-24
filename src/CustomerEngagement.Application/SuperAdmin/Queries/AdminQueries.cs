using MediatR;

namespace CustomerEngagement.Application.SuperAdmin.Queries;

public record GetAdminAccountsQuery(int Page, int PageSize) : IRequest<object>;

public record GetAdminAccountByIdQuery(long Id) : IRequest<object>;

public record GetAdminUsersQuery(int Page, int PageSize) : IRequest<object>;

public record GetAdminUserByIdQuery(long Id) : IRequest<object>;

public record GetAdminConfigQuery() : IRequest<object>;
