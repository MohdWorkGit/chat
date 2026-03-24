using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.HelpCenter;

public interface IArticleService
{
    Task<ArticleDto?> GetByIdAsync(int articleId, CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetBySlugAsync(int portalId, string slug, CancellationToken cancellationToken = default);

    Task<PaginatedResultDto<ArticleDto>> GetByPortalAsync(
        int portalId,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<PaginatedResultDto<ArticleDto>> SearchAsync(
        int portalId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<ArticleDto> CreateAsync(int portalId, CreateArticleRequest request, CancellationToken cancellationToken = default);

    Task<ArticleDto> UpdateAsync(int articleId, UpdateArticleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int articleId, CancellationToken cancellationToken = default);

    Task IncrementViewCountAsync(int articleId, CancellationToken cancellationToken = default);
}
