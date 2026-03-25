using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Sends a CSAT survey to the contact after a conversation is resolved.
/// Enqueued by Hangfire as a fire-and-forget job.
/// </summary>
public class CsatSurveyJob
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<CsatSurveyResponse> _csatRepository;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CsatSurveyJob> _logger;

    public CsatSurveyJob(
        IRepository<Conversation> conversationRepository,
        IRepository<Contact> contactRepository,
        IRepository<CsatSurveyResponse> csatRepository,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork,
        ILogger<CsatSurveyJob> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _csatRepository = csatRepository ?? throw new ArgumentNullException(nameof(csatRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation is null)
        {
            _logger.LogWarning("Conversation {ConversationId} not found for CSAT survey", conversationId);
            return;
        }

        var contact = await _contactRepository.GetByIdAsync(conversation.ContactId, cancellationToken);
        if (contact is null || string.IsNullOrEmpty(contact.Email))
        {
            _logger.LogDebug("Contact {ContactId} has no email, skipping CSAT survey", conversation.ContactId);
            return;
        }

        // Check if a survey already exists for this conversation
        var existing = await _csatRepository.FindAsync(
            s => s.ConversationId == conversationId, cancellationToken);
        if (existing.Any())
        {
            _logger.LogDebug("CSAT survey already exists for conversation {ConversationId}", conversationId);
            return;
        }

        // Create the survey response record (pending customer input)
        var survey = new CsatSurveyResponse
        {
            AccountId = conversation.AccountId,
            ConversationId = conversationId,
            ContactId = conversation.ContactId,
            AssigneeId = conversation.AssigneeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _csatRepository.AddAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send survey email
        try
        {
            await _emailSender.SendEmailAsync(
                contact.Email,
                contact.Name ?? contact.Email,
                $"How was your experience? (Conversation #{conversation.DisplayId})",
                $"<p>Please rate your experience by visiting: <a href='/survey/{survey.Id}'>Rate Now</a></p>",
                cancellationToken: cancellationToken);

            _logger.LogInformation("CSAT survey sent for conversation {ConversationId} to {Email}",
                conversationId, contact.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send CSAT survey email for conversation {ConversationId}", conversationId);
        }
    }
}
