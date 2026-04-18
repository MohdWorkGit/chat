using CustomerEngagement.Api.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/portals/{portalId:long}/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseRead)]
    public async Task<ActionResult> GetAll(long portalId)
    {
        var result = await _mediator.Send(new Application.Categories.Queries.GetCategoriesQuery(portalId));
        return Ok(result);
    }

    [HttpGet("{categoryId:long}")]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseRead)]
    public async Task<ActionResult> GetById(long portalId, long categoryId)
    {
        var result = await _mediator.Send(
            new Application.Categories.Queries.GetCategoryByIdQuery(portalId, categoryId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseWrite)]
    public async Task<ActionResult> Create(long portalId,
        [FromBody] Application.Categories.Commands.CreateCategoryCommand command)
    {
        command = command with { PortalId = portalId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { portalId, categoryId = result }, new { Id = result });
    }

    [HttpPut("{categoryId:long}")]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseWrite)]
    public async Task<ActionResult> Update(long portalId, long categoryId,
        [FromBody] Application.Categories.Commands.UpdateCategoryCommand command)
    {
        command = command with { PortalId = portalId, Id = categoryId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{categoryId:long}")]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseWrite)]
    public async Task<ActionResult> Delete(long portalId, long categoryId)
    {
        await _mediator.Send(new Application.Categories.Commands.DeleteCategoryCommand(portalId, categoryId));
        return NoContent();
    }
}
