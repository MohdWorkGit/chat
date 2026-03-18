using System.Text.RegularExpressions;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;

namespace CustomerEngagement.Application.Services.HelpCenter;

public class ArticleService : IArticleService
{
    private readonly IRepository<Article> _articleRepository;
    private readonly IRepository<Portal> _portalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArticleService(
        IRepository<Article> articleRepository,
        IRepository<Portal> portalRepository,
        IUnitOfWork unitOfWork)
    {
        _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
        _portalRepository = portalRepository ?? throw new ArgumentNullException(nameof(portalRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ArticleDto?> GetByIdAsync(int articleId, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(articleId, cancellationToken);
        return article is null ? null : MapToDto(article);
    }

    public async Task<ArticleDto?> GetBySlugAsync(int portalId, string slug, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.FindOneAsync(
            a => a.PortalId == portalId && a.Slug == slug, cancellationToken);
        return article is null ? null : MapToDto(article);
    }

    public async Task<PaginatedResultDto<ArticleDto>> GetByPortalAsync(
        int portalId,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _articleRepository.CountAsync(
            a => a.PortalId == portalId, cancellationToken);

        var articles = await _articleRepository.GetPagedAsync(
            page, pageSize,
            predicate: a => a.PortalId == portalId,
            orderBy: a => a.Position,
            ascending: true,
            cancellationToken: cancellationToken);

        return new PaginatedResultDto<ArticleDto>
        {
            Items = articles.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResultDto<ArticleDto>> SearchAsync(
        int portalId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var lowerQuery = query.ToLowerInvariant();

        var totalCount = await _articleRepository.CountAsync(
            a => a.PortalId == portalId &&
                 a.Status == ArticleStatus.Published &&
                 (a.Title.ToLower().Contains(lowerQuery) ||
                  (a.Description != null && a.Description.ToLower().Contains(lowerQuery)) ||
                  (a.Content != null && a.Content.ToLower().Contains(lowerQuery))),
            cancellationToken);

        var articles = await _articleRepository.GetPagedAsync(
            page, pageSize,
            predicate: a => a.PortalId == portalId &&
                            a.Status == ArticleStatus.Published &&
                            (a.Title.ToLower().Contains(lowerQuery) ||
                             (a.Description != null && a.Description.ToLower().Contains(lowerQuery)) ||
                             (a.Content != null && a.Content.ToLower().Contains(lowerQuery))),
            orderBy: a => a.Position,
            ascending: true,
            cancellationToken: cancellationToken);

        return new PaginatedResultDto<ArticleDto>
        {
            Items = articles.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ArticleDto> CreateAsync(int portalId, CreateArticleRequest request, CancellationToken cancellationToken = default)
    {
        var portal = await _portalRepository.GetByIdAsync(portalId, cancellationToken)
            ?? throw new InvalidOperationException($"Portal {portalId} not found.");

        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? request.Slug
            : GenerateSlug(request.Title);

        var article = new Article
        {
            PortalId = portalId,
            AccountId = portal.AccountId,
            Title = request.Title,
            Slug = slug,
            Content = request.Content,
            Description = request.Description,
            Status = (ArticleStatus)request.Status,
            CategoryId = request.CategoryId,
            AuthorId = request.AuthorId,
            Position = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _articleRepository.AddAsync(article, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(article);
    }

    public async Task<ArticleDto> UpdateAsync(int articleId, UpdateArticleRequest request, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new InvalidOperationException($"Article {articleId} not found.");

        if (request.Title is not null) article.Title = request.Title;
        if (request.Content is not null) article.Content = request.Content;
        if (request.Description is not null) article.Description = request.Description;
        if (request.Status.HasValue) article.Status = (ArticleStatus)request.Status.Value;
        if (request.CategoryId.HasValue) article.CategoryId = request.CategoryId.Value;
        article.UpdatedAt = DateTime.UtcNow;

        await _articleRepository.UpdateAsync(article, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(article);
    }

    public async Task DeleteAsync(int articleId, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new InvalidOperationException($"Article {articleId} not found.");

        await _articleRepository.DeleteAsync(article, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task IncrementViewCountAsync(int articleId, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(articleId, cancellationToken);
        if (article is null) return;

        // Note: Article entity doesn't have a ViewCount property in the BaseEntity,
        // but the DTO exposes it. For now we track via the UpdatedAt timestamp.
        // A dedicated ViewCount column should be added to the Article entity if needed.
        article.UpdatedAt = DateTime.UtcNow;
        await _articleRepository.UpdateAsync(article, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            PortalId = article.PortalId,
            AccountId = article.AccountId,
            Title = article.Title,
            Slug = article.Slug,
            Content = article.Content,
            Description = article.Description,
            Status = (int)article.Status,
            CategoryId = article.CategoryId,
            AuthorId = article.AuthorId,
            ViewCount = 0,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }
}
