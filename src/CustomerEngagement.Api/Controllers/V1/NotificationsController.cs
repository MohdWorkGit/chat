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
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(
            new Application.Notifications.Commands.MarkAllNotificationsReadCommand(accountId, userId));
        return Ok(result);
    }

    [HttpDelete("{notificationId:long}")]
    public async Task<ActionResult> Delete(long accountId, long notificationId)
    {
        await _mediator.Send(
            new Application.Notifications.Commands.DeleteNotificationCommand(accountId, notificationId));
        return NoContent();
    }

    [HttpPatch("{notificationId:long}/snooze")]
    public async Task<ActionResult> Snooze(long accountId, long notificationId,
        [FromBody] SnoozeNotificationRequest request)
    {
        var result = await _mediator.Send(
            new Application.Notifications.Commands.SnoozeNotificationCommand(
                accountId, notificationId, request.SnoozedUntil));
        return Ok(result);
    }

    [HttpGet("settings")]
    public async Task<ActionResult> GetSettings(long accountId)
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(
            new Application.Notifications.Queries.GetNotificationSettingsQuery(accountId, userId));
        return Ok(result);
    }

    [HttpPatch("settings")]
    public async Task<ActionResult> UpdateSettings(long accountId,
        [FromBody] UpdateNotificationSettingsRequest request)
    {
        var userId = long.Parse(User.FindFirst("uid")?.Value ?? "0");
        var result = await _mediator.Send(
            new Application.Notifications.Commands.UpdateNotificationSettingsCommand(
                accountId,
                userId,
                request.EmailConversationCreation,
                request.EmailConversationAssignment,
                request.EmailNewMessage,
                request.EmailMention,
                request.PushConversationCreation,
                request.PushConversationAssignment,
                request.PushNewMessage,
                request.PushMention));
        return Ok(result);
    }

    public record SnoozeNotificationRequest(DateTime SnoozedUntil);

    public record UpdateNotificationSettingsRequest(
        bool? EmailConversationCreation,
        bool? EmailConversationAssignment,
        bool? EmailNewMessage,
        bool? EmailMention,
        bool? PushConversationCreation,
        bool? PushConversationAssignment,
        bool? PushNewMessage,
        bool? PushMention);
}
