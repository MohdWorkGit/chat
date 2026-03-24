using System.Text.Json;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Interface for a GeoIP lookup service (e.g., MaxMind offline database).
/// </summary>
public interface IGeoIpService
{
    Task<GeoIpResult?> LookupAsync(string ipAddress, CancellationToken cancellationToken = default);
}

public record GeoIpResult(
    string? Country,
    string? CountryCode,
    string? City,
    string? Region,
    double? Latitude,
    double? Longitude,
    string? PostalCode,
    string? TimeZone);

public class ContactIpLookupJob
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IGeoIpService _geoIpService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ContactIpLookupJob> _logger;

    public ContactIpLookupJob(
        IRepository<Contact> contactRepository,
        IGeoIpService geoIpService,
        IUnitOfWork unitOfWork,
        ILogger<ContactIpLookupJob> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _geoIpService = geoIpService ?? throw new ArgumentNullException(nameof(geoIpService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Looks up the IP address for a contact and updates their location fields.
    /// Intended to be enqueued by Hangfire as a fire-and-forget job.
    /// </summary>
    public async Task ExecuteAsync(int contactId, string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var contact = await _contactRepository.GetByIdAsync(contactId, cancellationToken);
            if (contact is null)
            {
                _logger.LogWarning("Contact {ContactId} not found for IP lookup", contactId);
                return;
            }

            var result = await _geoIpService.LookupAsync(ipAddress, cancellationToken);
            if (result is null)
            {
                _logger.LogInformation("No GeoIP data found for IP {IpAddress}, contact {ContactId}",
                    ipAddress, contactId);
                return;
            }

            // Build location string from available components
            var locationParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(result.City))
                locationParts.Add(result.City);
            if (!string.IsNullOrWhiteSpace(result.Country))
                locationParts.Add(result.Country);

            contact.Location = locationParts.Count > 0 ? string.Join(", ", locationParts) : null;

            // Store country code and coordinates in additional attributes
            var additionalAttributes = !string.IsNullOrEmpty(contact.AdditionalAttributes)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(contact.AdditionalAttributes) ?? new()
                : new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(result.CountryCode))
                additionalAttributes["country_code"] = result.CountryCode;
            if (result.Latitude.HasValue)
                additionalAttributes["latitude"] = result.Latitude.Value;
            if (result.Longitude.HasValue)
                additionalAttributes["longitude"] = result.Longitude.Value;

            contact.AdditionalAttributes = JsonSerializer.Serialize(additionalAttributes);
            contact.UpdatedAt = DateTime.UtcNow;

            await _contactRepository.UpdateAsync(contact, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated contact {ContactId} location to {Location} (country: {CountryCode}) from IP {IpAddress}",
                contactId, contact.Location, result.CountryCode, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform IP lookup for contact {ContactId} with IP {IpAddress}",
                contactId, ipAddress);
        }
    }
}
