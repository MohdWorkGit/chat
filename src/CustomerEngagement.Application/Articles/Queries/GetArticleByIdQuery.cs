using CustomerEngagement.Application.Services.HelpCenter;
using MediatR;

namespace CustomerEngagement.Application.Articles.Queries;

public record GetArticleByIdQuery(long PortalId, long ArticleId) : IRequest<ArticleDto?>;

public class GetArticleByIdQueryHandler : IRequestHandler<GetArticleByIdQuery, ArticleDto?>
{
    private readonly IArticleService _articleService;

    public GetArticleByIdQueryHandler(IArticleService articleService)
    {
        _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
    }

    public async Task<ArticleDto?> Handle(GetArticleByIdQuery request, CancellationToken cancellationToken)
    {
        var article = await _articleService.GetByIdAsync((int)request.ArticleId, cancellationToken);
        if (article is not null && article.PortalId != (int)request.PortalId)
            return null;
        return article;
    }
}
