using System.Globalization;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Contacts;

public class ContactImportService : IContactImportService
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ContactImportService> _logger;

    public ContactImportService(
        IRepository<Contact> contactRepository,
        IUnitOfWork unitOfWork,
        ILogger<ContactImportService> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ContactImportResult> ImportFromCsvAsync(Stream csvStream, int accountId, CancellationToken cancellationToken = default)
    {
        var result = new ContactImportResult();

        using var reader = new StreamReader(csvStream);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Errors.Add("CSV file is empty or has no header row.");
            return result;
        }

        var headers = headerLine.Split(',').Select(h => h.Trim().ToLower(CultureInfo.InvariantCulture)).ToArray();
        var nameIndex = Array.IndexOf(headers, "name");
        var emailIndex = Array.IndexOf(headers, "email");
        var phoneIndex = Array.IndexOf(headers, "phone");
        var companyIndex = Array.IndexOf(headers, "company");
        var locationIndex = Array.IndexOf(headers, "location");

        var lineNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            lineNumber++;
            result.TotalRows++;

            if (string.IsNullOrWhiteSpace(line))
            {
                result.SkippedCount++;
                continue;
            }

            try
            {
                var fields = line.Split(',');

                var contact = new Contact
                {
                    AccountId = accountId,
                    Name = GetField(fields, nameIndex),
                    Email = GetField(fields, emailIndex),
                    Phone = GetField(fields, phoneIndex),
                    Company = GetField(fields, companyIndex),
                    Location = GetField(fields, locationIndex),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (string.IsNullOrWhiteSpace(contact.Email) && string.IsNullOrWhiteSpace(contact.Phone))
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Row {lineNumber}: Skipped - no email or phone provided.");
                    continue;
                }

                await _contactRepository.AddAsync(contact, cancellationToken);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.ErrorCount++;
                result.Errors.Add($"Row {lineNumber}: {ex.Message}");
                _logger.LogWarning(ex, "Failed to import contact at row {LineNumber}", lineNumber);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "CSV import completed for account {AccountId}: {Imported} imported, {Skipped} skipped, {Errors} errors",
            accountId, result.ImportedCount, result.SkippedCount, result.ErrorCount);

        return result;
    }

    private static string? GetField(string[] fields, int index)
    {
        if (index < 0 || index >= fields.Length)
            return null;

        var value = fields[index].Trim().Trim('"');
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
