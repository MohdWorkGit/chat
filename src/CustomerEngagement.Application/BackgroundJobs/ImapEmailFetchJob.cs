using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

public class ImapEmailFetchJob
{
    private readonly IEmailReceiver _imapEmailReceiver;
    private readonly IRepository<Inbox> _inboxRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImapEmailFetchJob> _logger;

    public ImapEmailFetchJob(
        IEmailReceiver imapEmailReceiver,
        IRepository<Inbox> inboxRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IRepository<Contact> contactRepository,
        IUnitOfWork unitOfWork,
        ILogger<ImapEmailFetchJob> logger)
    {
        _imapEmailReceiver = imapEmailReceiver ?? throw new ArgumentNullException(nameof(imapEmailReceiver));
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Periodically polls IMAP inboxes for new emails and creates conversations/messages.
    /// Intended to be scheduled by Hangfire as a recurring job.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting IMAP email fetch job");

        var emailInboxes = await _inboxRepository.FindAsync(
            i => i.ChannelType == "email",
            cancellationToken);

        foreach (var inbox in emailInboxes)
        {
            try
            {
                await FetchEmailsForInboxAsync(inbox, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch emails for inbox {InboxId} ({InboxName})",
                    inbox.Id, inbox.Name);
            }
        }

        _logger.LogInformation("IMAP email fetch job completed");
    }

    private async Task FetchEmailsForInboxAsync(Inbox inbox, CancellationToken cancellationToken)
    {
        var emails = await _imapEmailReceiver.FetchNewEmailsAsync(
            folderName: null,
            maxMessages: 50,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Fetched {Count} new emails for inbox {InboxId}", emails.Count, inbox.Id);

        foreach (var email in emails)
        {
            try
            {
                await ProcessReceivedEmailAsync(inbox, email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process email {MessageId} for inbox {InboxId}",
                    email.MessageId, inbox.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessReceivedEmailAsync(Inbox inbox, ReceivedEmail email, CancellationToken cancellationToken)
    {
        // Find or create the contact based on the sender's email address
        var contacts = await _contactRepository.FindAsync(
            c => c.AccountId == inbox.AccountId && c.Email == email.From,
            cancellationToken);

        var contact = contacts.FirstOrDefault();
        if (contact is null)
        {
            contact = new Contact
            {
                AccountId = inbox.AccountId,
                Email = email.From,
                Name = string.IsNullOrWhiteSpace(email.FromName) ? email.From : email.FromName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _contactRepository.AddAsync(contact, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Find an existing open conversation or create a new one
        Conversation? conversation = null;

        if (!string.IsNullOrEmpty(email.InReplyTo))
        {
            var existingConversations = await _conversationRepository.FindAsync(
                c => c.ContactId == contact.Id
                     && c.InboxId == inbox.Id
                     && c.Status == ConversationStatus.Open,
                cancellationToken);

            conversation = existingConversations.FirstOrDefault();
        }

        if (conversation is null)
        {
            var displayId = await ConversationDisplayIdGenerator.GetNextDisplayIdAsync(
                _conversationRepository, inbox.AccountId, cancellationToken);

            conversation = new Conversation
            {
                AccountId = inbox.AccountId,
                InboxId = inbox.Id,
                ContactId = contact.Id,
                DisplayId = displayId,
                Status = ConversationStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _conversationRepository.AddAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Create the message
        var message = new Message
        {
            ConversationId = conversation.Id,
            AccountId = inbox.AccountId,
            Content = email.HtmlBody ?? email.TextBody ?? string.Empty,
            ContentType = email.HtmlBody is not null ? "text/html" : "text/plain",
            MessageType = MessageType.Incoming,
            ExternalSourceIds = email.MessageId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        // Update conversation timestamp
        conversation.UpdatedAt = DateTime.UtcNow;
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        _logger.LogInformation(
            "Created message for conversation {ConversationId} from email {MessageId}",
            conversation.Id, email.MessageId);
    }
}
