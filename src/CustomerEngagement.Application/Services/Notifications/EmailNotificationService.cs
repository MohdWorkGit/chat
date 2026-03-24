using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Notifications;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IRepository<User> userRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IEmailSender emailSender,
        ILogger<EmailNotificationService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendAsync(EmailNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email notification to {To}: {Subject}", request.To, request.Subject);

            await _emailSender.SendEmailAsync(
                request.To,
                request.To,
                request.Subject,
                request.HtmlBody ?? request.Body,
                textBody: request.Body,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to {To}", request.To);
            throw;
        }
    }

    public async Task SendConversationAssignmentNotificationAsync(int userId, long conversationId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || string.IsNullOrEmpty(user.Email))
            return;

        var conversation = await _conversationRepository.GetByIdAsync((int)conversationId, cancellationToken);
        if (conversation is null)
            return;

        await SendAsync(new EmailNotificationRequest
        {
            To = user.Email,
            Subject = $"Conversation #{conversationId} has been assigned to you",
            Body = $"You have been assigned to conversation #{conversationId}.",
            TemplateName = "conversation_assignment",
            TemplateData = new Dictionary<string, object>
            {
                ["user_name"] = user.Name ?? "Agent",
                ["conversation_id"] = conversationId
            }
        }, cancellationToken);
    }

    public async Task SendNewMessageNotificationAsync(int userId, long conversationId, long messageId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || string.IsNullOrEmpty(user.Email))
            return;

        var message = await _messageRepository.GetByIdAsync((int)messageId, cancellationToken);
        if (message is null)
            return;

        await SendAsync(new EmailNotificationRequest
        {
            To = user.Email,
            Subject = $"New message in conversation #{conversationId}",
            Body = $"A new message has been added to conversation #{conversationId}.",
            TemplateName = "new_message",
            TemplateData = new Dictionary<string, object>
            {
                ["user_name"] = user.Name ?? "Agent",
                ["conversation_id"] = conversationId,
                ["message_preview"] = message.Content?.Length > 100
                    ? message.Content[..100] + "..."
                    : message.Content ?? string.Empty
            }
        }, cancellationToken);
    }

    public async Task SendMentionNotificationAsync(int userId, long conversationId, long messageId, int mentionedByUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || string.IsNullOrEmpty(user.Email))
            return;

        var mentionedBy = await _userRepository.GetByIdAsync(mentionedByUserId, cancellationToken);

        await SendAsync(new EmailNotificationRequest
        {
            To = user.Email,
            Subject = $"{mentionedBy?.Name ?? "Someone"} mentioned you in conversation #{conversationId}",
            Body = $"You were mentioned in conversation #{conversationId}.",
            TemplateName = "mention",
            TemplateData = new Dictionary<string, object>
            {
                ["user_name"] = user.Name ?? "Agent",
                ["mentioned_by"] = mentionedBy?.Name ?? "Someone",
                ["conversation_id"] = conversationId
            }
        }, cancellationToken);
    }
}
