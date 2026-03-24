using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Channels;

public interface IWebWidgetService
{
    Task<WebWidgetConfigDto> CreateWidgetAsync(int accountId, CreateWebWidgetRequest request, CancellationToken cancellationToken = default);

    Task<WebWidgetConfigDto?> GetConfigAsync(string widgetToken, CancellationToken cancellationToken = default);

    Task<bool> ValidateTokenAsync(string widgetToken, CancellationToken cancellationToken = default);
}
