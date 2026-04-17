using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Application.Services.Channels;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class OutboundEmailEventHandler : INotificationHandler<OutboundEmailEvent>
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<OutboundEmailEventHandler> _logger;

    public OutboundEmailEventHandler(
        IBackgroundJobClient jobClient,
        ILogger<OutboundEmailEventHandler> logger)
    {
        _jobClient = jobClient;
        _logger = logger;
    }

    public Task Handle(OutboundEmailEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Enqueuing email delivery for message {MessageId}", notification.MessageId);

        var messageId = (int)notification.MessageId;
        _jobClient.Enqueue<EmailDeliveryJob>(job =>
            job.ExecuteAsync(messageId, CancellationToken.None));

        return Task.CompletedTask;
    }
}
