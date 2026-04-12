using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Widget;

[ApiController]
[Route("api/v1/widget/conversations/{conversationId:long}/attachments")]
public class WidgetAttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WidgetAttachmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<ActionResult> UploadAttachment(
        long conversationId,
        [FromHeader(Name = "X-Website-Token")] string websiteToken,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        using var memStream = new MemoryStream();
        await file.CopyToAsync(memStream);
        var fileBytes = memStream.ToArray();

        var result = await _mediator.Send(new Application.Widget.Commands.UploadWidgetAttachmentCommand(
            WidgetToken: websiteToken,
            ConversationId: conversationId,
            FileName: file.FileName,
            ContentType: file.ContentType,
            FileBytes: fileBytes));

        return Ok(result);
    }
}
