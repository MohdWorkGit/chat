using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Public;

[ApiController]
[Route("api/v1/public/csat_survey")]
public class PublicCsatController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicCsatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{surveyToken}")]
    public async Task<ActionResult> GetSurvey(string surveyToken)
    {
        var result = await _mediator.Send(
            new Application.Public.Queries.GetPublicCsatSurveyQuery(surveyToken));
        return Ok(result);
    }

    [HttpPost("{surveyToken}")]
    public async Task<ActionResult> SubmitSurvey(string surveyToken,
        [FromBody] Application.Public.Commands.SubmitCsatSurveyCommand command)
    {
        command = command with { SurveyToken = surveyToken };
        await _mediator.Send(command);
        return Ok();
    }
}
