using MediatR;

namespace CustomerEngagement.Application.Agents.Commands;

public record CreateAgentCommand(long AccountId = 0, string Name = "", string? Email = null, string? Role = null) : IRequest<object>;

public record UpdateAgentCommand(long AccountId = 0, long Id = 0, string? Name = null, string? Email = null, string? Role = null) : IRequest<object>;

public record DeleteAgentCommand(long AccountId, long Id) : IRequest<object>;
