using MediatR;

namespace CustomerEngagement.Application.Webhooks.Commands;

public record CreateWebhookCommand(long AccountId = 0, string Url = "", string? Events = null) : IRequest<object>;

public record UpdateWebhookCommand(long AccountId = 0, long Id = 0, string? Url = null, string? Events = null) : IRequest<object>;

public record DeleteWebhookCommand(long AccountId, long Id) : IRequest<object>;
