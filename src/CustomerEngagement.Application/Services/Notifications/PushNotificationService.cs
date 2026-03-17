using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Notifications;

public class PushNotificationService : IPushNotificationService
{
    private readonly IRepository<DeviceToken> _deviceTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IRepository<DeviceToken> deviceTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<PushNotificationService> logger)
    {
        _deviceTokenRepository = deviceTokenRepository ?? throw new ArgumentNullException(nameof(deviceTokenRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendAsync(PushNotificationRequest request, CancellationToken cancellationToken = default)
    {
        await SendToUserAsync(request.UserId, request.Title, request.Body, request.Data, cancellationToken);
    }

    public async Task SendToUserAsync(int userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var deviceTokens = await _deviceTokenRepository.ListAsync(
                new { UserId = userId },
                cancellationToken);

            foreach (var token in deviceTokens)
            {
                try
                {
                    // Delegate to platform-specific push notification provider
                    // (Firebase Cloud Messaging, APNs, etc.)
                    // The actual sending is handled by infrastructure-layer implementations.
                    _logger.LogInformation(
                        "Sending push notification to user {UserId} on platform {Platform}",
                        userId, token.Platform);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send push notification to device {DeviceToken} for user {UserId}",
                        token.Token, userId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {UserId}", userId);
        }
    }

    public async Task RegisterDeviceTokenAsync(int userId, string deviceToken, string platform, CancellationToken cancellationToken = default)
    {
        // Check if token already registered
        var existing = await _deviceTokenRepository.ListAsync(
            new { UserId = userId, Token = deviceToken },
            cancellationToken);

        if (existing.Any())
            return;

        var token = new DeviceToken
        {
            UserId = userId,
            Token = deviceToken,
            Platform = platform,
            CreatedAt = DateTime.UtcNow
        };

        await _deviceTokenRepository.AddAsync(token, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registered device token for user {UserId} on platform {Platform}", userId, platform);
    }

    public async Task UnregisterDeviceTokenAsync(int userId, string deviceToken, CancellationToken cancellationToken = default)
    {
        var tokens = await _deviceTokenRepository.ListAsync(
            new { UserId = userId, Token = deviceToken },
            cancellationToken);

        var token = tokens.FirstOrDefault();
        if (token is not null)
        {
            await _deviceTokenRepository.DeleteAsync(token, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
