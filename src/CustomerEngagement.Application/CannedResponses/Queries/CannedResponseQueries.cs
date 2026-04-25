using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CannedResponses.Queries;

public record GetCannedResponsesQuery(long AccountId) : IRequest<object>;

public record GetCannedResponseByIdQuery(long AccountId, long Id) : IRequest<object>;

public record SearchCannedResponsesQuery(long AccountId, string Query) : IRequest<object>;

public class GetCannedResponsesQueryHandler : IRequestHandler<GetCannedResponsesQuery, object>
{
    private readonly IRepository<CannedResponse> _cannedResponseRepository;

    public GetCannedResponsesQueryHandler(IRepository<CannedResponse> cannedResponseRepository)
    {
        _cannedResponseRepository = cannedResponseRepository ?? throw new ArgumentNullException(nameof(cannedResponseRepository));
    }

    public async Task<object> Handle(GetCannedResponsesQuery request, CancellationToken cancellationToken)
    {
        var responses = await _cannedResponseRepository.FindAsync(
            cr => cr.AccountId == (int)request.AccountId, cancellationToken);

        return responses.Select(cr => new
        {
            cr.Id,
            cr.AccountId,
            cr.ShortCode,
            cr.Content,
            cr.CreatedAt,
            cr.UpdatedAt
        }).ToList();
    }
}

public class GetCannedResponseByIdQueryHandler : IRequestHandler<GetCannedResponseByIdQuery, object>
{
    private readonly IRepository<CannedResponse> _cannedResponseRepository;

    public GetCannedResponseByIdQueryHandler(IRepository<CannedResponse> cannedResponseRepository)
    {
        _cannedResponseRepository = cannedResponseRepository ?? throw new ArgumentNullException(nameof(cannedResponseRepository));
    }

    public async Task<object> Handle(GetCannedResponseByIdQuery request, CancellationToken cancellationToken)
    {
        var responses = await _cannedResponseRepository.FindAsync(
            cr => cr.AccountId == (int)request.AccountId && cr.Id == (int)request.Id,
            cancellationToken);

        var response = responses.FirstOrDefault();

        if (response is null)
            return new { Error = "Canned response not found" };

        return new
        {
            response.Id,
            response.AccountId,
            response.ShortCode,
            response.Content,
            response.CreatedAt,
            response.UpdatedAt
        };
    }
}

public class SearchCannedResponsesQueryHandler : IRequestHandler<SearchCannedResponsesQuery, object>
{
    private readonly IRepository<CannedResponse> _cannedResponseRepository;

    public SearchCannedResponsesQueryHandler(IRepository<CannedResponse> cannedResponseRepository)
    {
        _cannedResponseRepository = cannedResponseRepository ?? throw new ArgumentNullException(nameof(cannedResponseRepository));
    }

    public async Task<object> Handle(SearchCannedResponsesQuery request, CancellationToken cancellationToken)
    {
        var responses = await _cannedResponseRepository.FindAsync(
            cr => cr.AccountId == (int)request.AccountId &&
                  (cr.ShortCode.Contains(request.Query) || cr.Content.Contains(request.Query)),
            cancellationToken);

        return responses.Select(cr => new
        {
            cr.Id,
            cr.AccountId,
            cr.ShortCode,
            cr.Content,
            cr.CreatedAt,
            cr.UpdatedAt
        }).ToList();
    }
}
