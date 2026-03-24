using MediatR;

namespace CustomerEngagement.Application.Webhooks.Queries;

public record GetWebhooksQuery(long AccountId) : IRequest<object>;

public record GetWebhookByIdQuery(long AccountId, long Id) : IRequest<object>;
