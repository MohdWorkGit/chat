using MediatR;

namespace CustomerEngagement.Application.Teams.Queries;

public record GetTeamByIdQuery(long AccountId, long Id) : IRequest<object>;

public record GetTeamMembersQuery(long AccountId, long TeamId) : IRequest<object>;
