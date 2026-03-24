using MediatR;

namespace CustomerEngagement.Application.Agents.Queries;

public record GetAgentsQuery(long AccountId) : IRequest<object>;

public record GetAgentByIdQuery(long AccountId, long Id) : IRequest<object>;
