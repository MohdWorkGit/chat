using MediatR;

namespace CustomerEngagement.Application.Public.Queries;

public record GetPublicCsatSurveyQuery(string SurveyToken) : IRequest<object>;

public record GetPublicPortalQuery(string PortalSlug) : IRequest<object>;

public record GetPublicCategoriesQuery(string PortalSlug, string? Locale) : IRequest<object>;

public record GetPublicArticlesQuery(string PortalSlug, string? Locale, long? CategoryId, int Page, int PageSize) : IRequest<object>;

public record GetPublicArticleQuery(string PortalSlug, string ArticleSlug) : IRequest<object>;

public record GetPublicInboxQuery(string InboxIdentifier) : IRequest<object>;
