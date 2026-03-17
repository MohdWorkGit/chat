using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Enterprise.Saml.Services;

public record SamlConfigDto(
    int Id,
    int AccountId,
    string IdpEntityId,
    string IdpSsoTargetUrl,
    string IdpCertificate,
    string SpEntityId,
    string AssertionConsumerServiceUrl,
    string? NameIdentifierFormat,
    bool Enabled,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record SamlValidationResult(
    string Email,
    string NameId,
    Dictionary<string, string> Attributes,
    UserRole? MappedRole);

public interface ISamlAuthService
{
    Task<SamlConfigDto?> GetConfigAsync(int accountId, CancellationToken cancellationToken = default);
    Task<SamlConfigDto> CreateOrUpdateConfigAsync(int accountId, SamlConfigDto config, CancellationToken cancellationToken = default);
    Task<SamlValidationResult> ValidateAssertionAsync(int accountId, string samlResponse, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SamlRoleMappingDto>> GetRoleMappingsAsync(int samlConfigId, CancellationToken cancellationToken = default);
    Task<SamlRoleMappingDto> CreateRoleMappingAsync(int samlConfigId, string samlAttributeValue, UserRole userRole, CancellationToken cancellationToken = default);
    Task DeleteRoleMappingAsync(int roleMappingId, CancellationToken cancellationToken = default);
    Task<string> GenerateSpMetadataXmlAsync(int accountId, CancellationToken cancellationToken = default);
}

public record SamlRoleMappingDto(
    int Id,
    int SamlConfigId,
    string SamlAttributeValue,
    UserRole UserRole);
