using CustomerEngagement.Application.Services.HelpCenter;
using MediatR;

namespace CustomerEngagement.Application.Articles.Commands;

public record DeleteArticleCommand(long PortalId, long ArticleId) : IRequest;

public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand>
{
    private readonly IArticleService _articleService;

    public DeleteArticleCommandHandler(IArticleService articleService)
    {
        _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
    }

    public async Task Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await _articleService.GetByIdAsync((int)request.ArticleId, cancellationToken);
        if (article is not null && article.PortalId != (int)request.PortalId)
            throw new InvalidOperationException("Article does not belong to the specified portal.");

        await _articleService.DeleteAsync((int)request.ArticleId, cancellationToken);
    }
}
