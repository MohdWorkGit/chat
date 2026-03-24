using MediatR;

namespace CustomerEngagement.Application.Platform.Queries;

public record GetPlatformAccountsQuery(int Page, int PageSize) : IRequest<object>;

public record GetPlatformAccountByIdQuery(long Id) : IRequest<object>;

public record GetAgentBotsQuery(int Page, int PageSize) : IRequest<object>;

public record GetAgentBotByIdQuery(long Id) : IRequest<object>;

public record GetPlatformUsersQuery(int Page, int PageSize) : IRequest<object>;

public record GetPlatformUserByIdQuery(long Id) : IRequest<object>;
