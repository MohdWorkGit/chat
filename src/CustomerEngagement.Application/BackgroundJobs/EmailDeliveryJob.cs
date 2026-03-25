using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Delivers outgoing email messages via SMTP with retry and bounce handling.
/// Enqueued by Hangfire as a fire-and-forget job when an outgoing email message is created.
/// </summary>
public class EmailDeliveryJob
{
    private const int MaxRetries = 3;

    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailDeliveryJob> _logger;

    public EmailDeliveryJob(
        IRepository<Message> messageRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Contact> contactRepository,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork,
        ILogger<EmailDeliveryJob> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(int messageId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);
        if (message is null)
        {
            _logger.LogWarning("Message {MessageId} not found for email delivery", messageId);
            return;
        }

        var conversation = await _conversationRepository.GetByIdAsync(message.ConversationId, cancellationToken);
        if (conversation is null)
        {
            _logger.LogWarning("Conversation {ConversationId} not found for message {MessageId}",
                message.ConversationId, messageId);
            return;
        }

        var contact = await _contactRepository.GetByIdAsync(conversation.ContactId, cancellationToken);
        if (contact is null || string.IsNullOrEmpty(contact.Email))
        {
            _logger.LogWarning("Contact {ContactId} has no email for message delivery", conversation.ContactId);
            return;
        }

        Exception? lastException = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogInformation("Retrying email delivery for message {MessageId}, attempt {Attempt}", messageId, attempt);
                await Task.Delay(delay, cancellationToken);
            }

            try
            {
                await _emailSender.SendEmailAsync(
                    contact.Email,
                    contact.Name ?? contact.Email,
                    $"Re: Conversation #{conversation.DisplayId}",
                    message.Content ?? string.Empty,
                    cancellationToken: cancellationToken);

                message.Status = MessageStatus.Delivered;
                message.SentAt = DateTime.UtcNow;
                message.UpdatedAt = DateTime.UtcNow;
                await _messageRepository.UpdateAsync(message, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Email delivered for message {MessageId} to {Email}", messageId, contact.Email);
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Email delivery failed for message {MessageId}, attempt {Attempt}", messageId, attempt + 1);
            }
        }

        // Mark as failed after all retries exhausted
        message.Status = MessageStatus.Failed;
        message.UpdatedAt = DateTime.UtcNow;
        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogError(lastException, "Email delivery permanently failed for message {MessageId}", messageId);
    }
}
