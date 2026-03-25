using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Fetches and stores avatar images for contacts using Gravatar.
/// Enqueued by Hangfire when a new contact is created with an email address.
/// </summary>
public class AvatarFetchJob
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AvatarFetchJob> _logger;

    public AvatarFetchJob(
        IRepository<Contact> contactRepository,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        ILogger<AvatarFetchJob> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId, cancellationToken);
        if (contact is null)
        {
            _logger.LogWarning("Contact {ContactId} not found for avatar fetch", contactId);
            return;
        }

        if (string.IsNullOrEmpty(contact.Email) || !string.IsNullOrEmpty(contact.AvatarUrl))
        {
            return;
        }

        try
        {
            // Generate Gravatar URL from email hash
            var emailHash = ComputeMd5Hash(contact.Email.Trim().ToLowerInvariant());
            var gravatarUrl = $"https://www.gravatar.com/avatar/{emailHash}?d=404&s=200";

            using var client = _httpClientFactory.CreateClient("AvatarFetch");
            client.Timeout = TimeSpan.FromSeconds(10);

            using var response = await client.GetAsync(gravatarUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                contact.AvatarUrl = $"https://www.gravatar.com/avatar/{emailHash}?s=200";
                contact.UpdatedAt = DateTime.UtcNow;
                await _contactRepository.UpdateAsync(contact, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Avatar URL set for contact {ContactId}", contactId);
            }
            else
            {
                _logger.LogDebug("No Gravatar found for contact {ContactId} ({Email})", contactId, contact.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch avatar for contact {ContactId}", contactId);
        }
    }

    private static string ComputeMd5Hash(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
