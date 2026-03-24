using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Channels;

public interface IEmailChannelService
{
    Task ProcessInboundEmailAsync(InboundEmailRequest request, CancellationToken cancellationToken = default);

    Task SendOutboundEmailAsync(OutboundEmailRequest request, CancellationToken cancellationToken = default);
}
