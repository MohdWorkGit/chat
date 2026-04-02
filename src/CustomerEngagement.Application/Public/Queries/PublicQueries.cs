using System.Linq.Expressions;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Public.Queries;

public record GetPublicCsatSurveyQuery(string SurveyToken) : IRequest<object>;

public record GetPublicPortalQuery(string PortalSlug) : IRequest<object>;

public record GetPublicCategoriesQuery(string PortalSlug, string? Locale) : IRequest<object>;

public record GetPublicArticlesQuery(string PortalSlug, string? Locale, long? CategoryId, int Page, int PageSize) : IRequest<object>;

public record GetPublicArticleQuery(string PortalSlug, string ArticleSlug) : IRequest<object>;

public record GetPublicInboxQuery(string InboxIdentifier) : IRequest<object>;

public class GetPublicCsatSurveyQueryHandler : IRequestHandler<GetPublicCsatSurveyQuery, object>
{
    private readonly IRepository<CsatSurveyResponse> _repository;

    public GetPublicCsatSurveyQueryHandler(IRepository<CsatSurveyResponse> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPublicCsatSurveyQuery request, CancellationToken cancellationToken)
    {
        // Look up by ID parsed from the survey token
        if (!int.TryParse(request.SurveyToken, out var surveyId))
            return null!;

        var survey = await _repository.GetByIdAsync(surveyId, cancellationToken);
        if (survey is null)
            return null!;

        return new
        {
            survey.Id,
            survey.AccountId,
            survey.ConversationId,
            survey.Rating,
            survey.FeedbackText,
            survey.CreatedAt
        };
    }
}

public class GetPublicPortalQueryHandler : IRequestHandler<GetPublicPortalQuery, object>
{
    private readonly IRepository<Portal> _repository;

    public GetPublicPortalQueryHandler(IRepository<Portal> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPublicPortalQuery request, CancellationToken cancellationToken)
    {
        var portals = await _repository.FindAsync(
            p => p.Slug == request.PortalSlug && !p.Archived, cancellationToken);

        var portal = portals.FirstOrDefault();
        if (portal is null)
            return null!;

        return new
        {
            portal.Id,
            portal.Name,
            portal.Slug,
            portal.CustomDomain,
            portal.Color,
            portal.HeaderText,
            portal.PageTitle,
            portal.HomepageLink
        };
    }
}

public class GetPublicCategoriesQueryHandler : IRequestHandler<GetPublicCategoriesQuery, object>
{
    private readonly IRepository<Portal> _portalRepository;
    private readonly IRepository<Category> _categoryRepository;

    public GetPublicCategoriesQueryHandler(
        IRepository<Portal> portalRepository,
        IRepository<Category> categoryRepository)
    {
        _portalRepository = portalRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<object> Handle(GetPublicCategoriesQuery request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.FindAsync(
            p => p.Slug == request.PortalSlug && !p.Archived, cancellationToken);

        var portal = portals.FirstOrDefault();
        if (portal is null)
            return new { Data = Array.Empty<object>() };

        var locale = request.Locale;
        var portalId = portal.Id;

        var categories = await _categoryRepository.FindAsync(
            c => c.PortalId == portalId && (locale == null || c.Locale == locale), cancellationToken);

        return new
        {
            Data = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Slug,
                c.Position,
                c.Locale,
                c.ParentCategoryId
            }).OrderBy(c => c.Position)
        };
    }
}

public class GetPublicArticlesQueryHandler : IRequestHandler<GetPublicArticlesQuery, object>
{
    private readonly IRepository<Portal> _portalRepository;
    private readonly IRepository<Article> _articleRepository;

    public GetPublicArticlesQueryHandler(
        IRepository<Portal> portalRepository,
        IRepository<Article> articleRepository)
    {
        _portalRepository = portalRepository;
        _articleRepository = articleRepository;
    }

    public async Task<object> Handle(GetPublicArticlesQuery request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.FindAsync(
            p => p.Slug == request.PortalSlug && !p.Archived, cancellationToken);

        var portal = portals.FirstOrDefault();
        if (portal is null)
            return new { Data = Array.Empty<object>(), Meta = new { TotalCount = 0, Page = request.Page, PageSize = request.PageSize, TotalPages = 0 } };

        var portalId = portal.Id;
        var locale = request.Locale;
        var categoryId = request.CategoryId.HasValue ? (int)request.CategoryId.Value : (int?)null;

        Expression<Func<Article, bool>> predicate = a =>
            a.PortalId == portalId
            && a.Status == ArticleStatus.Published
            && (locale == null || a.Locale == locale)
            && (!categoryId.HasValue || a.CategoryId == categoryId.Value);

        var articles = await _articleRepository.GetPagedAsync(
            request.Page, request.PageSize, predicate, a => a.Position, ascending: true, cancellationToken);

        var totalCount = await _articleRepository.CountAsync(predicate, cancellationToken);

        return new
        {
            Data = articles.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.Slug,
                a.CategoryId,
                a.Position,
                a.Locale,
                a.CreatedAt,
                a.UpdatedAt
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

public class GetPublicArticleQueryHandler : IRequestHandler<GetPublicArticleQuery, object>
{
    private readonly IRepository<Portal> _portalRepository;
    private readonly IRepository<Article> _articleRepository;

    public GetPublicArticleQueryHandler(
        IRepository<Portal> portalRepository,
        IRepository<Article> articleRepository)
    {
        _portalRepository = portalRepository;
        _articleRepository = articleRepository;
    }

    public async Task<object> Handle(GetPublicArticleQuery request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.FindAsync(
            p => p.Slug == request.PortalSlug && !p.Archived, cancellationToken);

        var portal = portals.FirstOrDefault();
        if (portal is null)
            return null!;

        var portalId = portal.Id;
        var articleSlug = request.ArticleSlug;

        var articles = await _articleRepository.FindAsync(
            a => a.PortalId == portalId && a.Slug == articleSlug && a.Status == ArticleStatus.Published,
            cancellationToken);

        var article = articles.FirstOrDefault();
        if (article is null)
            return null!;

        return new
        {
            article.Id,
            article.Title,
            article.Content,
            article.Description,
            article.Slug,
            article.CategoryId,
            article.AuthorId,
            article.Position,
            article.Locale,
            article.CreatedAt,
            article.UpdatedAt
        };
    }
}

public class GetPublicInboxQueryHandler : IRequestHandler<GetPublicInboxQuery, object>
{
    private readonly IRepository<Inbox> _repository;

    public GetPublicInboxQueryHandler(IRepository<Inbox> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPublicInboxQuery request, CancellationToken cancellationToken)
    {
        var inboxes = await _repository.FindAsync(
            i => i.ChannelType == request.InboxIdentifier, cancellationToken);

        var inbox = inboxes.FirstOrDefault();
        if (inbox is null)
            return null!;

        return new
        {
            inbox.Id,
            inbox.Name,
            inbox.ChannelType,
            inbox.GreetingEnabled,
            inbox.GreetingMessage,
            inbox.EnableEmailCollect
        };
    }
}
