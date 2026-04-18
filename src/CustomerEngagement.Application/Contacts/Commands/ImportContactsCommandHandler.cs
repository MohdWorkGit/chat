using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Contacts.Commands;

public class ImportContactsCommandHandler : IRequestHandler<ImportContactsCommand, ImportContactsResult>
{
    private const int StatusPending = 0;

    private readonly IRepository<DataImport> _dataImportRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<ImportContactsCommandHandler> _logger;

    public ImportContactsCommandHandler(
        IRepository<DataImport> dataImportRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        IBackgroundJobClient jobClient,
        ILogger<ImportContactsCommandHandler> logger)
    {
        _dataImportRepository = dataImportRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _jobClient = jobClient;
        _logger = logger;
    }

    public async Task<ImportContactsResult> Handle(ImportContactsCommand request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
            return new ImportContactsResult(0, 0, new[] { "No file uploaded." });

        if (!request.File.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return new ImportContactsResult(0, 0, new[] { "Only .csv files are supported." });

        var dataImport = new DataImport
        {
            AccountId = (int)request.AccountId,
            DataType = "contacts",
            Status = StatusPending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dataImportRepository.AddAsync(dataImport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var storageKey = $"imports/{dataImport.Id}.csv";
        await using (var uploadStream = request.File.OpenReadStream())
        {
            await _storageService.UploadFileAsync(
                storageKey,
                uploadStream,
                request.File.ContentType ?? "text/csv",
                cancellationToken);
        }

        var dataImportId = dataImport.Id;
        _jobClient.Enqueue<DataImportJob>(job => job.ExecuteAsync(dataImportId, CancellationToken.None));

        _logger.LogInformation("Enqueued contact import job {DataImportId} for account {AccountId}",
            dataImportId, request.AccountId);

        return new ImportContactsResult(0, 0, Array.Empty<string>());
    }
}
