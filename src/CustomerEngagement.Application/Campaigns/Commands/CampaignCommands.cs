using MediatR;

namespace CustomerEngagement.Application.Campaigns.Commands;

public record CreateCampaignCommand(long AccountId = 0, string Title = "", string? Message = null) : IRequest<object>;

public record UpdateCampaignCommand(long AccountId = 0, long Id = 0, string? Title = null, string? Message = null) : IRequest<object>;

public record DeleteCampaignCommand(long AccountId, long Id) : IRequest<object>;
