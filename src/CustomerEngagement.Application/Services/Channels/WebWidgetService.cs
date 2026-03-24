using System.Security.Cryptography;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Entities.Channels;
using CustomerEngagement.Core.Interfaces;

namespace CustomerEngagement.Application.Services.Channels;

public class WebWidgetService : IWebWidgetService
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WebWidgetService(
        IRepository<ChannelWebWidget> widgetRepository,
        IUnitOfWork unitOfWork)
    {
        _widgetRepository = widgetRepository ?? throw new ArgumentNullException(nameof(widgetRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<WebWidgetConfigDto> CreateWidgetAsync(int accountId, CreateWebWidgetRequest request, CancellationToken cancellationToken = default)
    {
        var token = GenerateToken();

        var widget = new ChannelWebWidget
        {
            AccountId = accountId,
            InboxId = request.InboxId,
            WebsiteUrl = request.WebsiteUrl,
            WelcomeTitle = request.WelcomeTitle ?? "Welcome!",
            WelcomeTagline = request.WelcomeTagline ?? "We are here to help. Ask us anything.",
            WidgetColor = request.WidgetColor ?? "#1F93FF",
            WebsiteToken = token,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _widgetRepository.AddAsync(widget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(widget);
    }

    public async Task<WebWidgetConfigDto?> GetConfigAsync(string widgetToken, CancellationToken cancellationToken = default)
    {
        var widgets = await _widgetRepository.ListAsync(
            new { WebsiteToken = widgetToken },
            cancellationToken);

        var widget = widgets.FirstOrDefault();
        return widget is null ? null : MapToDto(widget);
    }

    public async Task<bool> ValidateTokenAsync(string widgetToken, CancellationToken cancellationToken = default)
    {
        var widgets = await _widgetRepository.ListAsync(
            new { WebsiteToken = widgetToken },
            cancellationToken);

        var widget = widgets.FirstOrDefault();
        return widget is not null && widget.IsEnabled;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(24);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
    }

    private static WebWidgetConfigDto MapToDto(ChannelWebWidget widget)
    {
        return new WebWidgetConfigDto
        {
            Id = widget.Id,
            AccountId = widget.AccountId,
            Token = widget.WebsiteToken,
            WebsiteUrl = widget.WebsiteUrl,
            WelcomeTitle = widget.WelcomeTitle,
            WelcomeTagline = widget.WelcomeTagline,
            WidgetColor = widget.WidgetColor,
            IsEnabled = widget.IsEnabled,
            CreatedAt = widget.CreatedAt
        };
    }
}
