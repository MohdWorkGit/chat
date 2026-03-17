namespace CustomerEngagement.Core.Interfaces;

public record ReceivedEmail(
    string MessageId,
    string From,
    string FromName,
    IReadOnlyList<string> To,
    string Subject,
    string? HtmlBody,
    string? TextBody,
    DateTimeOffset Date,
    string? InReplyTo,
    IReadOnlyList<ReceivedEmailAttachment> Attachments);

public record ReceivedEmailAttachment(string FileName, byte[] Content, string ContentType);

public interface IEmailReceiver
{
    Task<IReadOnlyList<ReceivedEmail>> FetchNewEmailsAsync(
        string? folderName = null,
        int maxMessages = 50,
        CancellationToken cancellationToken = default);

    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
