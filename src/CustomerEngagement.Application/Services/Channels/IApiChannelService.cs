using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Channels;

public interface IApiChannelService
{
    Task<ApiChannelMessageResult> ProcessInboundMessageAsync(ApiInboundMessageRequest request, CancellationToken cancellationToken = default);

    Task<ApiChannelMessageResult> SendOutboundMessageAsync(ApiOutboundMessageRequest request, CancellationToken cancellationToken = default);
}
