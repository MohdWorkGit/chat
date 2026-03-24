using MediatR;

namespace CustomerEngagement.Application.CannedResponses.Commands;

public record CreateCannedResponseCommand(long AccountId = 0, string ShortCode = "", string? Content = null) : IRequest<object>;

public record UpdateCannedResponseCommand(long AccountId = 0, long Id = 0, string? ShortCode = null, string? Content = null) : IRequest<object>;

public record DeleteCannedResponseCommand(long AccountId, long Id) : IRequest<object>;
