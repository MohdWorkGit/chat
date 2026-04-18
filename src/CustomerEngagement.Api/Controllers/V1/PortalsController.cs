using CustomerEngagement.Api.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/portals")]
[Authorize]
public class PortalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PortalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseRead)]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(new Application.Portals.Queries.GetPortalsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{portalId:long}")]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseRead)]
    public async Task<ActionResult> GetById(long portalId)
    {
        var result = await _mediator.Send(new Application.Portals.Queries.GetPortalByIdQuery(portalId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseWrite)]
    public async Task<ActionResult> Create([FromBody] Application.Portals.Commands.CreatePortalCommand command)
    {
        var accountId = HttpContext.Items.TryGetValue("AccountId", out var a) && a is long l
            ? (int)l
            : command.AccountId;

        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { portalId = result }, new { Id = result });
    }

    [HttpPut("{portalId:long}")]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseWrite)]
    public async Task<ActionResult> Update(long portalId,
        [FromBody] Application.Portals.Commands.UpdatePortalCommand command)
    {
        command = command with { Id = portalId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{portalId:long}")]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseWrite)]
    public async Task<ActionResult> Delete(long portalId)
    {
        await _mediator.Send(new Application.Portals.Commands.DeletePortalCommand(portalId));
        return NoContent();
    }

    [HttpPost("{portalId:int}/logo")]
    [Authorize(Policy = ResourcePolicies.KnowledgeBaseWrite)]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult> UploadLogo(int portalId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Error = "File is required." });

        using var memStream = new MemoryStream();
        await file.CopyToAsync(memStream);
        var bytes = memStream.ToArray();

        var result = await _mediator.Send(
            new Application.Portals.Commands.UploadPortalLogoCommand(
                portalId, bytes, file.FileName, file.ContentType));

        return Ok(result);
    }
}
