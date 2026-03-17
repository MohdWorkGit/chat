using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/portals/{portalId:long}/articles")]
[Authorize]
public class ArticlesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ArticlesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long portalId,
        [FromQuery] long? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Articles.Queries.GetArticlesQuery(portalId, categoryId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{articleId:long}")]
    public async Task<ActionResult> GetById(long portalId, long articleId)
    {
        var result = await _mediator.Send(
            new Application.Articles.Queries.GetArticleByIdQuery(portalId, articleId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(long portalId,
        [FromBody] Application.Articles.Commands.CreateArticleCommand command)
    {
        command = command with { PortalId = portalId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { portalId, articleId = result }, new { Id = result });
    }

    [HttpPut("{articleId:long}")]
    public async Task<ActionResult> Update(long portalId, long articleId,
        [FromBody] Application.Articles.Commands.UpdateArticleCommand command)
    {
        command = command with { PortalId = portalId, Id = articleId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{articleId:long}")]
    public async Task<ActionResult> Delete(long portalId, long articleId)
    {
        await _mediator.Send(new Application.Articles.Commands.DeleteArticleCommand(portalId, articleId));
        return NoContent();
    }
}
