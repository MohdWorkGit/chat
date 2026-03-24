namespace CustomerEngagement.Core.Interfaces;

public record EmailAttachment(string FileName, byte[] Content, string ContentType);

public interface IEmailSender
{
    Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        string? replyTo = null,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default);
}
