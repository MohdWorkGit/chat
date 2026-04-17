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

    [HttpGet("articles/search")]
    public async Task<ActionResult> SearchArticles(string portalSlug,
        [FromQuery] string q, [FromQuery] string? locale,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.SearchPublicArticlesQuery(portalSlug, q, locale, page, pageSize));
        return Ok(result);
    }

    [HttpGet("logo")]
    public async Task<ActionResult> GetLogo(string portalSlug)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicPortalLogoQuery(portalSlug));

        if (result is null)
            return NotFound();

        return File(result.Stream, result.ContentType);
    }

    [HttpGet("categories/{categorySlug}/related")]
    public async Task<ActionResult> GetRelatedCategories(string portalSlug, string categorySlug, [FromQuery] string? locale)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicRelatedCategoriesQuery(portalSlug, categorySlug, locale));
        return Ok(result);
    }

    [HttpGet("articles/{articleSlug}")]
    public async Task<ActionResult> GetArticle(string portalSlug, string articleSlug)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicArticleQuery(portalSlug, articleSlug));
        return Ok(result);
    }

    [HttpPost("articles/{articleSlug}/view")]
    public async Task<ActionResult> RecordArticleView(string portalSlug, string articleSlug)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.IncrementPublicArticleViewCommand(portalSlug, articleSlug));
        return Ok(result);
    }
}
