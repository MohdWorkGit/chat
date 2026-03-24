using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Channels;

public class EmailChannelService : IEmailChannelService
{
    private readonly IRepository<Inbox> _inboxRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<EmailChannelService> _logger;

    public EmailChannelService(
        IRepository<Inbox> inboxRepository,
        IRepository<Contact> contactRepository,
        IConversationService conversationService,
        IMessageService messageService,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<EmailChannelService> logger)
    {
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessInboundEmailAsync(InboundEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find the inbox by the To address
            var inboxes = await _inboxRepository.ListAsync(
                new { EmailAddress = request.To, ChannelType = "email" },
                cancellationToken);

            var inbox = inboxes.FirstOrDefault();
            if (inbox is null)
            {
                _logger.LogWarning("No inbox found for email address {ToAddress}", request.To);
                return;
            }

            // Find or create contact by email
            var contacts = await _contactRepository.ListAsync(
                new { Email = request.From, AccountId = inbox.AccountId },
                cancellationToken);

            var contact = contacts.FirstOrDefault();
            if (contact is null)
            {
                contact = new Contact
                {
                    AccountId = inbox.AccountId,
                    Email = request.From,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _contactRepository.AddAsync(contact, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Check if this is a reply to an existing conversation (via InReplyTo header)
            long conversationId;
            if (!string.IsNullOrEmpty(request.InReplyTo))
            {
                // Try to find existing conversation by message reference
                // For simplicity, create a new conversation if not found
                var conversation = await _conversationService.CreateAsync(inbox.AccountId, new CreateConversationRequest
                {
                    InboxId = inbox.Id,
                    ContactId = contact.Id,
                    InitialMessage = request.Subject
                }, cancellationToken);
                conversationId = conversation.Id;
            }
            else
            {
                var conversation = await _conversationService.CreateAsync(inbox.AccountId, new CreateConversationRequest
                {
                    InboxId = inbox.Id,
                    ContactId = contact.Id,
                    InitialMessage = request.Subject
                }, cancellationToken);
                conversationId = conversation.Id;
            }

            // Create the message
            var attachments = request.Attachments?.Select(a => new AttachmentRequest
            {
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.Content.Length,
                FileUrl = string.Empty // To be populated after file storage
            }).ToList();

            await _messageService.CreateAsync(conversationId, new CreateMessageRequest
            {
                Content = request.Body,
                MessageType = (int)MessageType.Incoming,
                ContentType = string.IsNullOrEmpty(request.HtmlBody) ? "text" : "html",
                Attachments = attachments
            }, cancellationToken);

            _logger.LogInformation("Processed inbound email from {From} to inbox {InboxId}", request.From, inbox.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process inbound email from {From}", request.From);
            throw;
        }
    }

    public async Task SendOutboundEmailAsync(OutboundEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // The actual email sending is delegated to an infrastructure email sender.
            // This service prepares the email data and publishes an event for the infrastructure layer.
            await _mediator.Publish(
                new OutboundEmailEvent(
                    request.ConversationId,
                    request.MessageId,
                    request.To,
                    request.Subject,
                    request.Body,
                    request.HtmlBody,
                    request.ReplyToMessageId),
                cancellationToken);

            _logger.LogInformation("Outbound email queued for conversation {ConversationId}", request.ConversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send outbound email for conversation {ConversationId}", request.ConversationId);
            throw;
        }
    }
}

public record OutboundEmailEvent(
    long ConversationId,
    long MessageId,
    string To,
    string Subject,
    string Body,
    string? HtmlBody,
    string? ReplyToMessageId) : INotification;
