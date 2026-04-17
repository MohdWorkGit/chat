using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Core.Events;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class ContactLifecycleEventHandler : INotificationHandler<ContactCreatedEvent>
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<ContactLifecycleEventHandler> _logger;

    public ContactLifecycleEventHandler(
        IBackgroundJobClient jobClient,
        ILogger<ContactLifecycleEventHandler> logger)
    {
        _jobClient = jobClient;
        _logger = logger;
    }

    public Task Handle(ContactCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Enqueuing avatar fetch for new contact {ContactId}", notification.ContactId);

        _jobClient.Enqueue<AvatarFetchJob>(job =>
            job.ExecuteAsync(notification.ContactId, CancellationToken.None));

        return Task.CompletedTask;
    }
}
