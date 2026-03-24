using MediatR;

namespace CustomerEngagement.Application.Teams.Commands;

public record DeleteTeamCommand(long AccountId, long Id) : IRequest<object>;

public record AddTeamMemberCommand(long AccountId = 0, long TeamId = 0, long UserId = 0) : IRequest<object>;

public record RemoveTeamMemberCommand(long AccountId, long TeamId, long UserId) : IRequest<object>;
