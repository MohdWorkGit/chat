using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using MediatR;

namespace CustomerEngagement.Application.Contacts.Queries;

public record SearchContactsQuery(
    long AccountId,
    string Query,
    int Page = 1,
    int PageSize = 25) : IRequest<PaginatedContactSearchResult>;

public class SearchContactsQueryHandler : IRequestHandler<SearchContactsQuery, PaginatedContactSearchResult>
{
    private readonly IContactSearchService _contactSearchService;

    public SearchContactsQueryHandler(IContactSearchService contactSearchService)
    {
        _contactSearchService = contactSearchService ?? throw new ArgumentNullException(nameof(contactSearchService));
    }

    public async Task<PaginatedContactSearchResult> Handle(SearchContactsQuery request, CancellationToken cancellationToken)
    {
        return await _contactSearchService.SearchAsync(
            (int)request.AccountId, request.Query, request.Page, request.PageSize, cancellationToken);
    }
}
