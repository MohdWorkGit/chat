using MediatR;

namespace CustomerEngagement.Application.Notifications.Queries;

public record GetNotificationsQuery(long AccountId, int Page, int PageSize) : IRequest<object>;
