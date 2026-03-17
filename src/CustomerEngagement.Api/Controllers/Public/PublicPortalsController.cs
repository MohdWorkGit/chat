using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Public;

[ApiController]
[Route("api/v1/public/portals/{portalSlug}")]
public class PublicPortalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicPortalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetPortal(string portalSlug)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicPortalQuery(portalSlug));
        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories(string portalSlug, [FromQuery] string? locale)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicCategoriesQuery(portalSlug, locale));
        return Ok(result);
    }

    [HttpGet("articles")]
    public async Task<ActionResult> GetArticles(string portalSlug,
        [FromQuery] string? locale, [FromQuery] long? categoryId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicArticlesQuery(portalSlug, locale, categoryId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("articles/{articleSlug}")]
    public async Task<ActionResult> GetArticle(string portalSlug, string articleSlug)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicArticleQuery(portalSlug, articleSlug));
        return Ok(result);
    }
}
