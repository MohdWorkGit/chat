using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/campaigns")]
[Authorize]
public class CampaignsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CampaignsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Campaigns.Queries.GetCampaignsQuery(accountId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{campaignId:long}")]
    public async Task<ActionResult> GetById(long accountId, long campaignId)
    {
        var result = await _mediator.Send(
            new Application.Campaigns.Queries.GetCampaignByIdQuery(accountId, campaignId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Create(long accountId,
        [FromBody] Application.Campaigns.Commands.CreateCampaignCommand command)
    {
        command = command with { AccountId = accountId };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { accountId, campaignId = result }, new { Id = result });
    }

    [HttpPut("{campaignId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Update(long accountId, long campaignId,
        [FromBody] Application.Campaigns.Commands.UpdateCampaignCommand command)
    {
        command = command with { AccountId = accountId, Id = campaignId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{campaignId:long}")]
    [Authorize(Policy = "Administrator")]
    public async Task<ActionResult> Delete(long accountId, long campaignId)
    {
        await _mediator.Send(
            new Application.Campaigns.Commands.DeleteCampaignCommand(accountId, campaignId));
        return NoContent();
    }
}
