using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Channels;

public class ApiChannelService : IApiChannelService
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApiChannelService> _logger;

    public ApiChannelService(
        IRepository<Contact> contactRepository,
        IConversationService conversationService,
        IMessageService messageService,
        IUnitOfWork unitOfWork,
        ILogger<ApiChannelService> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiChannelMessageResult> ProcessInboundMessageAsync(ApiInboundMessageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Resolve or create contact
            var contact = await ResolveContactAsync(request.AccountId, request.Contact, cancellationToken);

            // Create conversation
            var conversation = await _conversationService.CreateAsync(request.AccountId, new CreateConversationRequest
            {
                InboxId = request.InboxId,
                ContactId = contact.Id,
                InitialMessage = request.Content,
                AdditionalAttributes = request.AdditionalAttributes
            }, cancellationToken);

            // Create message
            var message = await _messageService.CreateAsync(conversation.Id, new CreateMessageRequest
            {
                Content = request.Content,
                MessageType = (int)MessageType.Incoming,
                ContentType = request.ContentType
            }, cancellationToken);

            return new ApiChannelMessageResult
            {
                Success = true,
                ConversationId = conversation.Id,
                MessageId = message.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process inbound API message for account {AccountId}", request.AccountId);
            return new ApiChannelMessageResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<ApiChannelMessageResult> SendOutboundMessageAsync(ApiOutboundMessageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await _messageService.CreateAsync(request.ConversationId, new CreateMessageRequest
            {
                Content = request.Content,
                MessageType = (int)MessageType.Outgoing,
                ContentType = request.ContentType,
                SenderId = request.SenderId,
                SenderType = "User"
            }, cancellationToken);

            return new ApiChannelMessageResult
            {
                Success = true,
                ConversationId = request.ConversationId,
                MessageId = message.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send outbound API message for conversation {ConversationId}", request.ConversationId);
            return new ApiChannelMessageResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private async Task<Contact> ResolveContactAsync(int accountId, ContactIdentifier? identifier, CancellationToken cancellationToken)
    {
        if (identifier is not null)
        {
            // Try to find existing contact by email or phone
            if (!string.IsNullOrEmpty(identifier.Email))
            {
                var contacts = await _contactRepository.ListAsync(
                    new { Email = identifier.Email, AccountId = accountId },
                    cancellationToken);

                var existing = contacts.FirstOrDefault();
                if (existing is not null)
                    return existing;
            }

            if (!string.IsNullOrEmpty(identifier.Phone))
            {
                var contacts = await _contactRepository.ListAsync(
                    new { Phone = identifier.Phone, AccountId = accountId },
                    cancellationToken);

                var existing = contacts.FirstOrDefault();
                if (existing is not null)
                    return existing;
            }
        }

        // Create new contact
        var newContact = new Contact
        {
            AccountId = accountId,
            Name = identifier?.Name,
            Email = identifier?.Email,
            Phone = identifier?.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contactRepository.AddAsync(newContact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newContact;
    }
}
