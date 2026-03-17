using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Contacts;

public interface IContactImportService
{
    Task<ContactImportResult> ImportFromCsvAsync(Stream csvStream, int accountId, CancellationToken cancellationToken = default);
}

public class ContactImportResult
{
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
