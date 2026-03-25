using System.Text;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Sends a conversation transcript via email to specified recipients.
/// Enqueued by Hangfire when an agent triggers "send email transcript".
/// </summary>
public class EmailTranscriptJob
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailTranscriptJob> _logger;

    public EmailTranscriptJob(
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IRepository<Contact> contactRepository,
        IEmailSender emailSender,
        ILogger<EmailTranscriptJob> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(int conversationId, string recipientEmail, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation is null)
        {
            _logger.LogWarning("Conversation {ConversationId} not found for transcript", conversationId);
            return;
        }

        var messages = await _messageRepository.FindAsync(
            m => m.ConversationId == conversationId && !m.Private,
            cancellationToken);

        var contact = await _contactRepository.GetByIdAsync(conversation.ContactId, cancellationToken);

        var transcript = new StringBuilder();
        transcript.AppendLine($"Conversation #{conversation.DisplayId} Transcript");
        transcript.AppendLine($"Contact: {contact?.Name ?? "Unknown"} ({contact?.Email ?? "N/A"})");
        transcript.AppendLine($"Status: {conversation.Status}");
        transcript.AppendLine(new string('-', 50));
        transcript.AppendLine();

        foreach (var message in messages.OrderBy(m => m.CreatedAt))
        {
            var sender = message.SenderType == "Contact"
                ? (contact?.Name ?? "Contact")
                : "Agent";
            transcript.AppendLine($"[{message.CreatedAt:yyyy-MM-dd HH:mm:ss}] {sender}:");
            transcript.AppendLine(message.Content ?? "(no content)");
            transcript.AppendLine();
        }

        try
        {
            await _emailSender.SendEmailAsync(
                recipientEmail,
                recipientEmail,
                $"Transcript: Conversation #{conversation.DisplayId}",
                $"<pre>{transcript}</pre>",
                transcript.ToString(),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Email transcript sent for conversation {ConversationId} to {Email}",
                conversationId, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email transcript for conversation {ConversationId}", conversationId);
        }
    }
}
