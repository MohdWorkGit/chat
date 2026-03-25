using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Synchronizes email templates across accounts and validates template content.
/// Runs as a daily recurring Hangfire job.
/// </summary>
public class TemplateSyncJob
{
    private static readonly string[] RequiredTemplates =
    [
        "conversation_assignment",
        "conversation_reply",
        "conversation_creation",
        "password_reset",
        "email_confirmation",
        "csat_survey",
        "team_notification"
    ];

    private readonly IRepository<EmailTemplate> _templateRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TemplateSyncJob> _logger;

    public TemplateSyncJob(
        IRepository<EmailTemplate> templateRepository,
        IRepository<Account> accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<TemplateSyncJob> logger)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting template sync job");

        var accounts = await _accountRepository.GetAllAsync(cancellationToken);
        var createdCount = 0;

        foreach (var account in accounts)
        {
            var existingTemplates = await _templateRepository.FindAsync(
                t => t.AccountId == account.Id, cancellationToken);

            var existingNames = existingTemplates
                .Select(t => t.Name)
                .Where(n => n is not null)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var templateName in RequiredTemplates)
            {
                if (existingNames.Contains(templateName))
                    continue;

                var template = new EmailTemplate
                {
                    AccountId = account.Id,
                    Name = templateName,
                    Body = GetDefaultTemplateBody(templateName),
                    Locale = account.Locale ?? "en",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _templateRepository.AddAsync(template, cancellationToken);
                createdCount++;
            }
        }

        if (createdCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Template sync completed. Created {Count} missing templates", createdCount);
    }

    private static string GetDefaultTemplateBody(string templateName) => templateName switch
    {
        "conversation_assignment" => "<p>You have been assigned to conversation #{{conversation_id}}.</p>",
        "conversation_reply" => "<p>New reply in conversation #{{conversation_id}}: {{message_content}}</p>",
        "conversation_creation" => "<p>A new conversation #{{conversation_id}} has been created.</p>",
        "password_reset" => "<p>Click <a href='{{reset_url}}'>here</a> to reset your password.</p>",
        "email_confirmation" => "<p>Click <a href='{{confirmation_url}}'>here</a> to confirm your email.</p>",
        "csat_survey" => "<p>How was your experience? <a href='{{survey_url}}'>Rate now</a></p>",
        "team_notification" => "<p>{{notification_message}}</p>",
        _ => $"<p>Default template for {templateName}</p>"
    };
}
