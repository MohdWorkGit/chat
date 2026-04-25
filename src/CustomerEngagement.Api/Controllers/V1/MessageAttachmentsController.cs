using System.Security.Claims;
using CustomerEngagement.Api.Authorization;
using CustomerEngagement.Application.Messages.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.V1;

/// <summary>
/// Agent-side attachment upload. Creates an outgoing message with an
/// attachment on the given conversation. Uses multipart/form-data so
/// binary payloads can be sent directly without base64 overhead.
/// </summary>
[ApiController]
[Route("api/v1/accounts/{accountId:long}/conversations/{conversationId:long}/attachments")]
[Authorize(Policy = ResourcePolicies.ConversationWrite)]
public class MessageAttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessageAttachmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB, matches the validator
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public async Task<ActionResult> Upload(
        long accountId,
        long conversationId,
        IFormFile file,
        [FromForm] string? caption,
        [FromForm] bool isPrivate = false)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        using var memStream = new MemoryStream();
        await file.CopyToAsync(memStream);
        var fileBytes = memStream.ToArray();

        var senderId = ResolveSenderId(User);

        var message = await _mediator.Send(new UploadMessageAttachmentCommand(
            AccountId: accountId,
            ConversationId: conversationId,
            SenderId: senderId,
            Caption: caption,
            IsPrivate: isPrivate,
            FileName: file.FileName,
            ContentType: file.ContentType,
            FileBytes: fileBytes));

        return Ok(message);
    }

    private static int? ResolveSenderId(ClaimsPrincipal user)
    {
        var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst("user_id")?.Value;

        return int.TryParse(sub, out var id) ? id : null;
    }
}
