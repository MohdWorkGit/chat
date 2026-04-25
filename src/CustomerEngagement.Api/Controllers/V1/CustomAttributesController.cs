using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/custom_attribute_definitions")]
[Authorize]
public class CustomAttributesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomAttributesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId, [FromQuery] string? appliedTo)
    {
        var result = await _mediator.Send(
            new Application.CustomAttributes.Queries.GetCustomAttributesQuery(accountId, appliedTo));
        return Ok(result);
    }

    [HttpGet("{attributeId:long}")]
    public async Task<ActionResult> GetById(long accountId, long attributeId)
    {
        var result = await _mediator.Send(
            new Application.CustomAttributes.Queries.GetCustomAttributeByIdQuery(accountId, attributeId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.CustomAttributes.Commands.CreateCustomAttributeCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{attributeId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Update(long accountId, long attributeId,
        [FromBody] Application.CustomAttributes.Commands.UpdateCustomAttributeCommand command)
    {
        command = command with { AccountId = accountId, Id = attributeId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{attributeId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Delete(long accountId, long attributeId)
    {
        await _mediator.Send(
            new Application.CustomAttributes.Commands.DeleteCustomAttributeCommand(accountId, attributeId));
        return NoContent();
    }
}
