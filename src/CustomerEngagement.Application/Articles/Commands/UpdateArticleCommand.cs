using CustomerEngagement.Application.Services.HelpCenter;
using MediatR;

namespace CustomerEngagement.Application.Articles.Commands;

public record UpdateArticleCommand(
    long PortalId,
    long Id,
    string? Title,
    string? Content,
    string? Description,
    int? Status,
    long? CategoryId) : IRequest;

public class UpdateArticleCommandHandler : IRequestHandler<UpdateArticleCommand>
{
    private readonly IArticleService _articleService;

    public UpdateArticleCommandHandler(IArticleService articleService)
    {
        _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
    }

    public async Task Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateArticleRequest
        {
            Title = request.Title,
            Content = request.Content,
            Description = request.Description,
            Status = request.Status,
            CategoryId = request.CategoryId.HasValue ? (int)request.CategoryId.Value : null
        };

        await _articleService.UpdateAsync((int)request.Id, updateRequest, cancellationToken);
    }
}
