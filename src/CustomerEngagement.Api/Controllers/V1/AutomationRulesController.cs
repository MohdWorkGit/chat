using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/automation_rules")]
[Authorize]
public class AutomationRulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AutomationRulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.AutomationRules.Queries.GetAutomationRulesQuery(accountId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{ruleId:long}")]
    public async Task<ActionResult> GetById(long accountId, long ruleId)
    {
        var result = await _mediator.Send(
            new Application.AutomationRules.Queries.GetAutomationRuleByIdQuery(accountId, ruleId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.AutomationRules.Commands.CreateAutomationRuleCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, ruleId = result }, new { Id = result });
    }

    [HttpPut("{ruleId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Update(long accountId, long ruleId,
        [FromBody] Application.AutomationRules.Commands.UpdateAutomationRuleCommand command)
    {
        command = command with { AccountId = accountId, Id = ruleId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{ruleId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Delete(long accountId, long ruleId)
    {
        await _mediator.Send(
            new Application.AutomationRules.Commands.DeleteAutomationRuleCommand(accountId, ruleId));
        return NoContent();
    }
}
