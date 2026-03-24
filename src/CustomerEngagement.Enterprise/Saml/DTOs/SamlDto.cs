using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Enterprise.Saml.DTOs;

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

public record SamlRoleMappingDto(
    int Id,
    int SamlConfigId,
    string SamlAttributeValue,
    UserRole UserRole);
