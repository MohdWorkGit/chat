using System.Globalization;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Interface for file storage operations (e.g., MinIO/S3).
/// </summary>
public interface IStorageService
{
    Task<Stream> DownloadFileAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string key, CancellationToken cancellationToken = default);
    Task<string> UploadFileAsync(string key, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
}

public class DataImportJob
{
    private const int StatusProcessing = 1;
    private const int StatusCompleted = 2;
    private const int StatusFailed = 3;

    private readonly IRepository<DataImport> _dataImportRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataImportJob> _logger;

    public DataImportJob(
        IRepository<DataImport> dataImportRepository,
        IRepository<Contact> contactRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        ILogger<DataImportJob> logger)
    {
        _dataImportRepository = dataImportRepository ?? throw new ArgumentNullException(nameof(dataImportRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a data import by reading CSV from storage and creating contacts in bulk.
    /// Intended to be enqueued by Hangfire as a fire-and-forget job.
    /// </summary>
    public async Task ExecuteAsync(int dataImportId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting data import job for import {DataImportId}", dataImportId);

        var dataImport = await _dataImportRepository.GetByIdAsync(dataImportId, cancellationToken);
        if (dataImport is null)
        {
            _logger.LogWarning("DataImport {DataImportId} not found", dataImportId);
            return;
        }

        try
        {
            dataImport.Status = StatusProcessing;
            dataImport.UpdatedAt = DateTime.UtcNow;
            await _dataImportRepository.UpdateAsync(dataImport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var filePath = $"imports/{dataImportId}.csv";
            await using var fileStream = await _storageService.DownloadFileAsync(filePath, cancellationToken);
            using var reader = new StreamReader(fileStream);

            var headerLine = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                throw new InvalidOperationException("CSV file is empty or has no header row.");
            }

            var headers = headerLine.Split(',')
                .Select(h => h.Trim().ToLower(CultureInfo.InvariantCulture))
                .ToArray();

            var nameIndex = Array.IndexOf(headers, "name");
            var emailIndex = Array.IndexOf(headers, "email");
            var phoneIndex = Array.IndexOf(headers, "phone");
            var companyIndex = Array.IndexOf(headers, "company");
            var locationIndex = Array.IndexOf(headers, "location");

            // Count total lines first for progress tracking
            var lines = new List<string>();
            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }

            dataImport.TotalRecords = lines.Count;
            await _dataImportRepository.UpdateAsync(dataImport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var processedCount = 0;

            foreach (var csvLine in lines)
            {
                try
                {
                    var fields = csvLine.Split(',');

                    var contact = new Contact
                    {
                        AccountId = dataImport.AccountId,
                        Name = GetField(fields, nameIndex),
                        Email = GetField(fields, emailIndex),
                        Phone = GetField(fields, phoneIndex),
                        Location = GetField(fields, locationIndex),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    if (string.IsNullOrWhiteSpace(contact.Email) && string.IsNullOrWhiteSpace(contact.Phone))
                    {
                        _logger.LogDebug("Skipping row with no email or phone in import {DataImportId}", dataImportId);
                        continue;
                    }

                    await _contactRepository.AddAsync(contact, cancellationToken);
                    processedCount++;

                    // Update progress periodically (every 100 records)
                    if (processedCount % 100 == 0)
                    {
                        dataImport.ProcessedRecords = processedCount;
                        dataImport.UpdatedAt = DateTime.UtcNow;
                        await _dataImportRepository.UpdateAsync(dataImport, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import row in import {DataImportId}", dataImportId);
                }
            }

            // Final save
            dataImport.ProcessedRecords = processedCount;
            dataImport.Status = StatusCompleted;
            dataImport.UpdatedAt = DateTime.UtcNow;
            await _dataImportRepository.UpdateAsync(dataImport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Data import {DataImportId} completed. Processed {Processed}/{Total} records",
                dataImportId, processedCount, dataImport.TotalRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data import {DataImportId} failed", dataImportId);

            dataImport.Status = StatusFailed;
            dataImport.UpdatedAt = DateTime.UtcNow;
            await _dataImportRepository.UpdateAsync(dataImport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static string? GetField(string[] fields, int index)
    {
        if (index < 0 || index >= fields.Length)
            return null;

        var value = fields[index].Trim().Trim('"');
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
