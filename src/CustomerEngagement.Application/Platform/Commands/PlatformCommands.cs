using MediatR;

namespace CustomerEngagement.Application.Platform.Commands;

public record CreatePlatformAccountCommand(string Name = "") : IRequest<object>;

public record UpdatePlatformAccountCommand(long Id = 0, string? Name = null) : IRequest<object>;

public record DeletePlatformAccountCommand(long Id) : IRequest<object>;

public record CreateAgentBotCommand(string Name = "") : IRequest<object>;

public record UpdateAgentBotCommand(long Id = 0, string? Name = null) : IRequest<object>;

public record DeleteAgentBotCommand(long Id) : IRequest<object>;

public record CreatePlatformUserCommand(string Name = "", string? Email = null) : IRequest<object>;

public record UpdatePlatformUserCommand(long Id = 0, string? Name = null, string? Email = null) : IRequest<object>;

public record DeletePlatformUserCommand(long Id) : IRequest<object>;
