using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/custom_filters")]
[Authorize]
public class CustomFiltersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomFiltersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId, [FromQuery] string? filterType)
    {
        var result = await _mediator.Send(
            new Application.CustomFilters.Queries.GetCustomFiltersQuery(accountId, filterType));
        return Ok(result);
    }

    [HttpGet("{filterId:long}")]
    public async Task<ActionResult> GetById(long accountId, long filterId)
    {
        var result = await _mediator.Send(
            new Application.CustomFilters.Queries.GetCustomFilterByIdQuery(accountId, filterId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.CustomFilters.Commands.CreateCustomFilterCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, filterId = result }, new { Id = result });
    }

    [HttpPut("{filterId:long}")]
    public async Task<ActionResult> Update(long accountId, long filterId,
        [FromBody] Application.CustomFilters.Commands.UpdateCustomFilterCommand command)
    {
        command = command with { AccountId = accountId, Id = filterId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{filterId:long}")]
    public async Task<ActionResult> Delete(long accountId, long filterId)
    {
        await _mediator.Send(
            new Application.CustomFilters.Commands.DeleteCustomFilterCommand(accountId, filterId));
        return NoContent();
    }
}
