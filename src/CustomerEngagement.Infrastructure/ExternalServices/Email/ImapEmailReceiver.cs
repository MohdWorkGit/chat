using CustomerEngagement.Core.Interfaces;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CustomerEngagement.Infrastructure.ExternalServices.Email;

public class ImapEmailReceiver : IEmailReceiver
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImapEmailReceiver> _logger;

    public ImapEmailReceiver(IConfiguration configuration, ILogger<ImapEmailReceiver> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReceivedEmail>> FetchNewEmailsAsync(
        string? folderName = null,
        int maxMessages = 50,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["IMAP_HOST"] ?? "localhost";
        var port = int.Parse(_configuration["IMAP_PORT"] ?? "993");
        var username = _configuration["IMAP_USERNAME"]
            ?? throw new InvalidOperationException("IMAP_USERNAME is not configured.");
        var password = _configuration["IMAP_PASSWORD"]
            ?? throw new InvalidOperationException("IMAP_PASSWORD is not configured.");
        var useSsl = bool.Parse(_configuration["IMAP_USE_SSL"] ?? "true");

        var emails = new List<ReceivedEmail>();

        using var client = new ImapClient();
        try
        {
            var secureSocketOptions = useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto;
            await client.ConnectAsync(host, port, secureSocketOptions, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);

            var folder = string.IsNullOrEmpty(folderName)
                ? client.Inbox
                : await client.GetFolderAsync(folderName, cancellationToken);

            await folder.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

            var uids = await folder.SearchAsync(SearchQuery.NotSeen, cancellationToken);
            var messagesToFetch = uids.Take(maxMessages).ToList();

            _logger.LogInformation("Found {Count} unseen emails in {Folder}", messagesToFetch.Count, folder.FullName);

            foreach (var uid in messagesToFetch)
            {
                var message = await folder.GetMessageAsync(uid, cancellationToken);
                var receivedEmail = ConvertToReceivedEmail(message);
                emails.Add(receivedEmail);

                await folder.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
            }

            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch emails from IMAP server {Host}", host);
            throw;
        }

        return emails;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var host = _configuration["IMAP_HOST"] ?? "localhost";
        var port = int.Parse(_configuration["IMAP_PORT"] ?? "993");
        var username = _configuration["IMAP_USERNAME"] ?? string.Empty;
        var password = _configuration["IMAP_PASSWORD"] ?? string.Empty;
        var useSsl = bool.Parse(_configuration["IMAP_USE_SSL"] ?? "true");

        using var client = new ImapClient();
        try
        {
            var secureSocketOptions = useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto;
            await client.ConnectAsync(host, port, secureSocketOptions, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IMAP connection test failed for {Host}:{Port}", host, port);
            return false;
        }
    }

    private static ReceivedEmail ConvertToReceivedEmail(MimeMessage message)
    {
        var from = message.From.Mailboxes.FirstOrDefault();
        var attachments = new List<ReceivedEmailAttachment>();

        foreach (var attachment in message.Attachments)
        {
            if (attachment is MimePart mimePart)
            {
                using var stream = new MemoryStream();
                mimePart.Content.DecodeTo(stream);
                attachments.Add(new ReceivedEmailAttachment(
                    mimePart.FileName ?? "attachment",
                    stream.ToArray(),
                    mimePart.ContentType.MimeType));
            }
        }

        return new ReceivedEmail(
            MessageId: message.MessageId ?? string.Empty,
            From: from?.Address ?? string.Empty,
            FromName: from?.Name ?? string.Empty,
            To: message.To.Mailboxes.Select(m => m.Address).ToList(),
            Subject: message.Subject ?? string.Empty,
            HtmlBody: message.HtmlBody,
            TextBody: message.TextBody,
            Date: message.Date,
            InReplyTo: message.InReplyTo,
            Attachments: attachments);
    }
}
