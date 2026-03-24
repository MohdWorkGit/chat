using MediatR;

namespace CustomerEngagement.Application.Macros.Commands;

public record CreateMacroCommand(long AccountId = 0, string Name = "") : IRequest<object>;

public record UpdateMacroCommand(long AccountId = 0, long Id = 0, string? Name = null) : IRequest<object>;

public record DeleteMacroCommand(long AccountId, long Id) : IRequest<object>;

public record ExecuteMacroCommand(long AccountId = 0, long MacroId = 0, long ConversationId = 0) : IRequest<object>;
