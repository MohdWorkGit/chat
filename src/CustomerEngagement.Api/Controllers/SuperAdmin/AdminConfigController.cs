using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.SuperAdmin;

[ApiController]
[Route("api/v1/super_admin/config")]
[Authorize(Roles = "SuperAdmin")]
public class AdminConfigController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminConfigController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetConfig()
    {
        var result = await _mediator.Send(new Application.SuperAdmin.Queries.GetAdminConfigQuery());
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateConfig(
        [FromBody] Application.SuperAdmin.Commands.UpdateAdminConfigCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
