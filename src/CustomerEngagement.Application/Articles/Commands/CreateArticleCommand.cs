using CustomerEngagement.Application.Services.HelpCenter;
using MediatR;

namespace CustomerEngagement.Application.Articles.Commands;

public record CreateArticleCommand(
    long PortalId,
    string Title,
    string? Slug,
    string? Content,
    string? Description,
    int Status,
    long? CategoryId,
    int AuthorId) : IRequest<long>;

public class CreateArticleCommandHandler : IRequestHandler<CreateArticleCommand, long>
{
    private readonly IArticleService _articleService;

    public CreateArticleCommandHandler(IArticleService articleService)
    {
        _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
    }

    public async Task<long> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateArticleRequest
        {
            Title = request.Title,
            Slug = request.Slug,
            Content = request.Content,
            Description = request.Description,
            Status = request.Status,
            CategoryId = request.CategoryId.HasValue ? (int)request.CategoryId.Value : null,
            AuthorId = request.AuthorId
        };

        var result = await _articleService.CreateAsync((int)request.PortalId, createRequest, cancellationToken);
        return result.Id;
    }
}
