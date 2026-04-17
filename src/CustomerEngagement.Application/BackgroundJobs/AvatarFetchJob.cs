using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

/// <summary>
/// Generates an offline, initials-based SVG avatar for a contact. The platform
/// is designed to run air-gapped, so this job cannot reach Gravatar or any
/// external image host; instead it produces a deterministic, self-contained
/// data-URL avatar keyed to the contact's name (or email local-part).
/// Enqueued by Hangfire when a new contact is created.
/// </summary>
public class AvatarFetchJob
{
    private static readonly string[] PaletteColors =
    {
        "#1abc9c", "#2ecc71", "#3498db", "#9b59b6",
        "#e67e22", "#e74c3c", "#16a085", "#27ae60",
        "#2980b9", "#8e44ad", "#d35400", "#c0392b"
    };

    private readonly IRepository<Contact> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AvatarFetchJob> _logger;

    public AvatarFetchJob(
        IRepository<Contact> contactRepository,
        IUnitOfWork unitOfWork,
        ILogger<AvatarFetchJob> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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

        if (!string.IsNullOrEmpty(contact.AvatarUrl))
            return;

        var seed = !string.IsNullOrWhiteSpace(contact.Name)
            ? contact.Name!
            : contact.Email?.Split('@').FirstOrDefault() ?? $"contact{contact.Id}";

        var initials = ExtractInitials(seed);
        var color = PickColor(seed);
        contact.AvatarUrl = BuildSvgDataUri(initials, color);
        contact.UpdatedAt = DateTime.UtcNow;

        await _contactRepository.UpdateAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated offline avatar for contact {ContactId}", contactId);
    }

    private static string ExtractInitials(string source)
    {
        var parts = source.Split(new[] { ' ', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return "?";

        if (parts.Length == 1)
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();

        return string.Concat(parts[0][0], parts[^1][0]).ToUpperInvariant();
    }

    private static string PickColor(string seed)
    {
        unchecked
        {
            var hash = 0;
            foreach (var ch in seed)
                hash = hash * 31 + ch;
            var index = Math.Abs(hash) % PaletteColors.Length;
            return PaletteColors[index];
        }
    }

    private static string BuildSvgDataUri(string initials, string backgroundColor)
    {
        var safeInitials = System.Net.WebUtility.HtmlEncode(initials);
        var svg =
            $"<svg xmlns='http://www.w3.org/2000/svg' width='200' height='200' viewBox='0 0 200 200'>" +
            $"<rect width='200' height='200' fill='{backgroundColor}'/>" +
            $"<text x='50%' y='50%' text-anchor='middle' dominant-baseline='central' " +
            $"font-family='-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif' " +
            $"font-size='88' font-weight='600' fill='#ffffff'>{safeInitials}</text>" +
            $"</svg>";

        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{base64}";
    }
}
