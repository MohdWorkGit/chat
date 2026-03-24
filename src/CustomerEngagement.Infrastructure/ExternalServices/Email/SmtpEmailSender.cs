using CustomerEngagement.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CustomerEngagement.Infrastructure.ExternalServices.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        string? replyTo = null,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["SMTP_HOST"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["SMTP_PORT"] ?? "587");
        var smtpUsername = _configuration["SMTP_USERNAME"];
        var smtpPassword = _configuration["SMTP_PASSWORD"];
        var fromEmail = _configuration["SMTP_FROM_EMAIL"] ?? "noreply@example.com";
        var fromName = _configuration["SMTP_FROM_NAME"] ?? "Customer Engagement";
        var useSsl = bool.Parse(_configuration["SMTP_USE_SSL"] ?? "true");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        if (!string.IsNullOrEmpty(replyTo))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(replyTo));
        }

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody
        };

        if (attachments is not null)
        {
            foreach (var attachment in attachments)
            {
                bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
            }
        }

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var secureSocketOptions = useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
            {
                await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
    }

    public async Task SendBulkEmailAsync(
        IEnumerable<(string Email, string Name)> recipients,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var (email, name) in recipients)
        {
            try
            {
                await SendEmailAsync(email, name, subject, htmlBody, textBody, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bulk email to {Email}", email);
            }
        }
    }
}
