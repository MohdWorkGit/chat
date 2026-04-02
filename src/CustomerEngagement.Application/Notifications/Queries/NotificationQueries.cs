using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Notifications.Queries;

public record GetNotificationsQuery(long AccountId, int Page, int PageSize) : IRequest<object>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, object>
{
    private readonly IRepository<Notification> _notificationRepository;

    public GetNotificationsQueryHandler(IRepository<Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public async Task<object> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            predicate: n => n.AccountId == (int)request.AccountId,
            orderBy: n => n.CreatedAt,
            ascending: false,
            cancellationToken: cancellationToken);

        var totalCount = await _notificationRepository.CountAsync(
            n => n.AccountId == (int)request.AccountId, cancellationToken);

        return new
        {
            Data = notifications.Select(n => new
            {
                n.Id,
                n.AccountId,
                n.UserId,
                n.NotificationType,
                n.PrimaryActorType,
                n.PrimaryActorId,
                n.SecondaryActorType,
                n.SecondaryActorId,
                n.ReadAt,
                n.CreatedAt
            }).ToList(),
            Meta = new
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        };
    }
}
