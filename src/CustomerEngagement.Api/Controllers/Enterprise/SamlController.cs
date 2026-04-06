using CustomerEngagement.Core.Enums;
using CustomerEngagement.Enterprise.Saml.DTOs;
using CustomerEngagement.Enterprise.Saml.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerEngagement.Api.Controllers.Enterprise;

[ApiController]
[Route("api/v1/accounts/{accountId:int}/saml")]
[Authorize]
public class SamlController : ControllerBase
{
    private readonly ISamlAuthService _samlAuthService;

    public SamlController(ISamlAuthService samlAuthService)
    {
        _samlAuthService = samlAuthService;
    }

    [HttpGet("config")]
    public async Task<ActionResult<SamlConfigDto>> GetConfig(
        int accountId,
        CancellationToken cancellationToken)
    {
        var config = await _samlAuthService.GetConfigAsync(accountId, cancellationToken);
        if (config is null)
        {
            return NotFound();
        }

        return Ok(config);
    }

    [HttpPut("config")]
    public async Task<ActionResult<SamlConfigDto>> UpsertConfig(
        int accountId,
        [FromBody] SamlConfigBody body,
        CancellationToken cancellationToken)
    {
        var dto = new SamlConfigDto(
            0,
            accountId,
            body.IdpEntityId,
            body.IdpSsoTargetUrl,
            body.IdpCertificate,
            body.SpEntityId,
            body.AssertionConsumerServiceUrl,
            body.NameIdentifierFormat,
            body.Enabled,
            DateTime.UtcNow,
            DateTime.UtcNow);

        var saved = await _samlAuthService.CreateOrUpdateConfigAsync(accountId, dto, cancellationToken);
        return Ok(saved);
    }

    [HttpGet("metadata")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSpMetadata(
        int accountId,
        CancellationToken cancellationToken)
    {
        var xml = await _samlAuthService.GenerateSpMetadataXmlAsync(accountId, cancellationToken);
        return Content(xml, "application/samlmetadata+xml");
    }

    [HttpPost("acs")]
    [AllowAnonymous]
    public async Task<ActionResult<SamlValidationResult>> AssertionConsumerService(
        int accountId,
        [FromForm] string SAMLResponse,
        CancellationToken cancellationToken)
    {
        var result = await _samlAuthService.ValidateAssertionAsync(accountId, SAMLResponse, cancellationToken);
        return Ok(result);
    }

    [HttpGet("configs/{samlConfigId:int}/role_mappings")]
    public async Task<ActionResult<IReadOnlyList<SamlRoleMappingDto>>> GetRoleMappings(
        int accountId,
        int samlConfigId,
        CancellationToken cancellationToken)
    {
        var mappings = await _samlAuthService.GetRoleMappingsAsync(samlConfigId, cancellationToken);
        return Ok(mappings);
    }

    [HttpPost("configs/{samlConfigId:int}/role_mappings")]
    public async Task<ActionResult<SamlRoleMappingDto>> CreateRoleMapping(
        int accountId,
        int samlConfigId,
        [FromBody] CreateRoleMappingBody body,
        CancellationToken cancellationToken)
    {
        var created = await _samlAuthService.CreateRoleMappingAsync(
            samlConfigId, body.SamlAttributeValue, body.UserRole, cancellationToken);
        return Ok(created);
    }

    [HttpDelete("role_mappings/{roleMappingId:int}")]
    public async Task<ActionResult> DeleteRoleMapping(
        int accountId,
        int roleMappingId,
        CancellationToken cancellationToken)
    {
        await _samlAuthService.DeleteRoleMappingAsync(roleMappingId, cancellationToken);
        return NoContent();
    }
}

public record SamlConfigBody(
    string IdpEntityId,
    string IdpSsoTargetUrl,
    string IdpCertificate,
    string SpEntityId,
    string AssertionConsumerServiceUrl,
    string? NameIdentifierFormat,
    bool Enabled);

public record CreateRoleMappingBody(string SamlAttributeValue, UserRole UserRole);
