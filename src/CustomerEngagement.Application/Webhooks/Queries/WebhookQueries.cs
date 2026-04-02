using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Webhooks.Queries;

public record GetWebhooksQuery(long AccountId) : IRequest<object>;

public record GetWebhookByIdQuery(long AccountId, long Id) : IRequest<object>;

public class GetWebhooksQueryHandler : IRequestHandler<GetWebhooksQuery, object>
{
    private readonly IRepository<Webhook> _repository;

    public GetWebhooksQueryHandler(IRepository<Webhook> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetWebhooksQuery request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        var webhooks = await _repository.FindAsync(w => w.AccountId == accountId, cancellationToken);

        return new
        {
            Data = webhooks.Select(w => new
            {
                w.Id,
                w.AccountId,
                w.Url,
                w.SubscribedEvents,
                w.HmacToken,
                w.IsActive,
                w.CreatedAt,
                w.UpdatedAt
            })
        };
    }
}

public class GetWebhookByIdQueryHandler : IRequestHandler<GetWebhookByIdQuery, object>
{
    private readonly IRepository<Webhook> _repository;

    public GetWebhookByIdQueryHandler(IRepository<Webhook> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetWebhookByIdQuery request, CancellationToken cancellationToken)
    {
        var webhook = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (webhook is null || webhook.AccountId != (int)request.AccountId)
            return null!;

        return new
        {
            webhook.Id,
            webhook.AccountId,
            webhook.Url,
            webhook.SubscribedEvents,
            webhook.HmacToken,
            webhook.IsActive,
            webhook.CreatedAt,
            webhook.UpdatedAt
        };
    }
}
