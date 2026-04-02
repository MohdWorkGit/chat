using System.Linq.Expressions;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Campaigns.Queries;

public record GetCampaignsQuery(long AccountId, int Page, int PageSize) : IRequest<object>;

public record GetCampaignByIdQuery(long AccountId, long Id) : IRequest<object>;

public class GetCampaignsQueryHandler : IRequestHandler<GetCampaignsQuery, object>
{
    private readonly IRepository<Campaign> _repository;

    public GetCampaignsQueryHandler(IRepository<Campaign> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var accountId = (int)request.AccountId;
        Expression<Func<Campaign, bool>> predicate = c => c.AccountId == accountId;

        var campaigns = await _repository.GetPagedAsync(
            request.Page, request.PageSize, predicate, c => c.CreatedAt, ascending: false, cancellationToken);

        var totalCount = await _repository.CountAsync(predicate, cancellationToken);

        return new
        {
            Data = campaigns.Select(c => new
            {
                c.Id,
                c.AccountId,
                c.Title,
                c.Description,
                c.Message,
                c.CampaignType,
                c.Status,
                c.InboxId,
                c.ScheduledAt,
                c.Audience,
                c.Enabled,
                c.CreatedAt,
                c.UpdatedAt
            }),
            Meta = new
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        };
    }
}

public class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, object>
{
    private readonly IRepository<Campaign> _repository;

    public GetCampaignByIdQueryHandler(IRepository<Campaign> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (campaign is null || campaign.AccountId != (int)request.AccountId)
            return null!;

        return new
        {
            campaign.Id,
            campaign.AccountId,
            campaign.Title,
            campaign.Description,
            campaign.Message,
            campaign.CampaignType,
            campaign.Status,
            campaign.InboxId,
            campaign.ScheduledAt,
            campaign.Audience,
            campaign.Enabled,
            campaign.CreatedAt,
            campaign.UpdatedAt
        };
    }
}
