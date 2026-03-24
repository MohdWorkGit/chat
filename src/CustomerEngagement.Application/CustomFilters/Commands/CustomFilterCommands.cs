using MediatR;

namespace CustomerEngagement.Application.CustomFilters.Commands;

public record CreateCustomFilterCommand(long AccountId = 0, string Name = "", string? FilterType = null) : IRequest<object>;

public record UpdateCustomFilterCommand(long AccountId = 0, long Id = 0, string? Name = null) : IRequest<object>;

public record DeleteCustomFilterCommand(long AccountId, long Id) : IRequest<object>;
