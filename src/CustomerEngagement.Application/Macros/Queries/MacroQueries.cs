using MediatR;

namespace CustomerEngagement.Application.Macros.Queries;

public record GetMacrosQuery(long AccountId, int Page, int PageSize) : IRequest<object>;

public record GetMacroByIdQuery(long AccountId, long Id) : IRequest<object>;
