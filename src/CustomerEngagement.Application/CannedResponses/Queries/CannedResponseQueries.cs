using MediatR;

namespace CustomerEngagement.Application.CannedResponses.Queries;

public record GetCannedResponsesQuery(long AccountId) : IRequest<object>;

public record GetCannedResponseByIdQuery(long AccountId, long Id) : IRequest<object>;

public record SearchCannedResponsesQuery(long AccountId, string Query) : IRequest<object>;
