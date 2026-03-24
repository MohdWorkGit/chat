using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Integrations;

public interface IRasaNluService
{
    Task<BotResponse> ProcessMessageAsync(BotMessageRequest message, CancellationToken cancellationToken = default);
}
