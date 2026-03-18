using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.HelpCenter;
using MediatR;

namespace CustomerEngagement.Application.Articles.Queries;

public record GetArticlesQuery(
    long PortalId,
    long? CategoryId,
    int Page = 1,
    int PageSize = 25) : IRequest<PaginatedResultDto<ArticleDto>>;

public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, PaginatedResultDto<ArticleDto>>
{
    private readonly IArticleService _articleService;

    public GetArticlesQueryHandler(IArticleService articleService)
    {
        _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
    }

    public async Task<PaginatedResultDto<ArticleDto>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        return await _articleService.GetByPortalAsync(
            (int)request.PortalId, request.Page, request.PageSize, cancellationToken);
    }
}
