using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts/{accountId:long}/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(long accountId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(
            new Application.Notifications.Queries.GetNotificationsQuery(accountId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount(long accountId)
    {
        var result = await _mediator.Send(
            new Application.Notifications.Queries.GetUnreadNotificationCountQuery(accountId));
        return Ok(result);
    }

    [HttpPost("{notificationId:long}/read")]
    public async Task<ActionResult> MarkRead(long accountId, long notificationId)
    {
        await _mediator.Send(
            new Application.Notifications.Commands.MarkNotificationReadCommand(accountId, notificationId));
        return Ok();
    }

    [HttpPost("read_all")]
    public async Task<ActionResult> MarkAllRead(long accountId)
    {
        await _mediator.Send(
            new Application.Notifications.Commands.MarkAllNotificationsReadCommand(accountId));
        return Ok();
    }

    [HttpDelete("{notificationId:long}")]
    public async Task<ActionResult> Delete(long accountId, long notificationId)
    {
        await _mediator.Send(
            new Application.Notifications.Commands.DeleteNotificationCommand(accountId, notificationId));
        return NoContent();
    }
}
