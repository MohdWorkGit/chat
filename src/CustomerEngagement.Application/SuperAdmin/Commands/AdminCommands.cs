using MediatR;

namespace CustomerEngagement.Application.SuperAdmin.Commands;

public record CreateAdminAccountCommand(string Name = "") : IRequest<object>;

public record UpdateAdminAccountCommand(long Id = 0, string? Name = null) : IRequest<object>;

public record DeleteAdminAccountCommand(long Id) : IRequest<object>;

public record CreateAdminUserCommand(string Name = "", string? Email = null) : IRequest<object>;

public record UpdateAdminUserCommand(long Id = 0, string? Name = null, string? Email = null) : IRequest<object>;

public record DeleteAdminUserCommand(long Id) : IRequest<object>;

public record UpdateAdminConfigCommand(string? Key = null, string? Value = null) : IRequest<object>;
