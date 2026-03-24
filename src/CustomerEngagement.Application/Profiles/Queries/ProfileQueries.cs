using MediatR;

namespace CustomerEngagement.Application.Profiles.Queries;

public record GetProfileQuery(long UserId) : IRequest<object>;
