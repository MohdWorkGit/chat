using System.Text.Json;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.EventHandlers;

public sealed class CampaignEventHandler : INotificationHandler<ContactCreatedEvent>
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<CampaignEventHandler> _logger;

    public CampaignEventHandler(
        IRepository<Contact> contactRepository,
        IRepository<Campaign> campaignRepository,
        IRepository<Message> messageRepository,
        IRepository<Conversation> conversationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<CampaignEventHandler> logger)
    {
        _contactRepository = contactRepository;
        _campaignRepository = campaignRepository;
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(ContactCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Evaluating campaigns for new Contact {ContactId} in Account {AccountId}",
            notification.ContactId, notification.AccountId);

        var contact = await _contactRepository.GetByIdAsync(notification.ContactId, cancellationToken);
        if (contact is null)
            return;

        // Find active ongoing campaigns for this account
        var campaigns = await _campaignRepository.FindAsync(
            c => c.AccountId == notification.AccountId
                 && c.Enabled
                 && c.CampaignType == CampaignType.Ongoing,
            cancellationToken);

        foreach (var campaign in campaigns)
        {
            if (!IsContactInAudience(contact, campaign))
                continue;

            _logger.LogInformation("Contact {ContactId} matches campaign {CampaignId} ({CampaignTitle}), triggering delivery",
                notification.ContactId, campaign.Id, campaign.Title);

            try
            {
                await DeliverCampaignMessageAsync(contact, campaign, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering campaign {CampaignId} to Contact {ContactId}",
                    campaign.Id, notification.ContactId);
            }
        }
    }

    private static bool IsContactInAudience(Contact contact, Campaign campaign)
    {
        // If no audience filter is defined, the campaign targets all contacts
        if (string.IsNullOrWhiteSpace(campaign.Audience))
            return true;

        try
        {
            var audienceFilters = JsonSerializer.Deserialize<List<AudienceFilter>>(campaign.Audience);
            if (audienceFilters is null || audienceFilters.Count == 0)
                return true;

            // Evaluate each filter against the contact attributes
            foreach (var filter in audienceFilters)
            {
                if (!EvaluateFilter(contact, filter))
                    return false;
            }

            return true;
        }
        catch
        {
            // If audience filter can't be parsed, skip this campaign
            return false;
        }
    }

    private static bool EvaluateFilter(Contact contact, AudienceFilter filter)
    {
        var attributeValue = filter.Attribute?.ToLowerInvariant() switch
        {
            "email" => contact.Email,
            "phone_number" => contact.Phone,
            "name" => contact.Name,
            _ => null
        };

        if (attributeValue is null)
            return false;

        return filter.Operator?.ToLowerInvariant() switch
        {
            "equals" => string.Equals(attributeValue, filter.Value, StringComparison.OrdinalIgnoreCase),
            "contains" => attributeValue.Contains(filter.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "starts_with" => attributeValue.StartsWith(filter.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private async Task DeliverCampaignMessageAsync(Contact contact, Campaign campaign, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(campaign.Message))
            return;

        // Find or create a conversation for this contact in the campaign's inbox
        var existingConversations = await _conversationRepository.FindAsync(
            c => c.ContactId == contact.Id
                 && c.InboxId == campaign.InboxId
                 && c.AccountId == campaign.AccountId,
            cancellationToken);

        var conversation = existingConversations.FirstOrDefault();

        if (conversation is null)
        {
            var displayId = await ConversationDisplayIdGenerator.GetNextDisplayIdAsync(
                _conversationRepository, campaign.AccountId, cancellationToken);

            conversation = new Conversation
            {
                AccountId = campaign.AccountId,
                InboxId = campaign.InboxId,
                ContactId = contact.Id,
                DisplayId = displayId,
                Status = ConversationStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var message = new Message
        {
            ConversationId = conversation.Id,
            AccountId = campaign.AccountId,
            Content = campaign.Message,
            ContentType = "text",
            MessageType = MessageType.Outgoing,
            SenderType = "Campaign",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} message delivered to Contact {ContactId} in Conversation {ConversationId}",
            campaign.Id, contact.Id, conversation.Id);
    }

    private sealed class AudienceFilter
    {
        public string? Attribute { get; set; }
        public string? Operator { get; set; }
        public string? Value { get; set; }
    }
}
