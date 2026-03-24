using MediatR;

namespace CustomerEngagement.Application.Campaigns.Queries;

public record GetCampaignsQuery(long AccountId, int Page, int PageSize) : IRequest<object>;

public record GetCampaignByIdQuery(long AccountId, long Id) : IRequest<object>;
