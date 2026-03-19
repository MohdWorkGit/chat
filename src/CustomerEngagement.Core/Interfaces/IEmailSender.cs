namespace CustomerEngagement.Core.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default);
}
