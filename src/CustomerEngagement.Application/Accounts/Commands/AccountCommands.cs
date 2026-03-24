using MediatR;

namespace CustomerEngagement.Application.Accounts.Commands;

public record CreateAccountCommand(string Name = "", string? Locale = null) : IRequest<object>;

public record UpdateAccountCommand(long Id = 0, string? Name = null, string? Locale = null) : IRequest<object>;

public record DeleteAccountCommand(long Id) : IRequest<object>;
