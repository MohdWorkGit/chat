using CustomerEngagement.Core.Enums;
using CustomerEngagement.Enterprise.Saml.DTOs;

namespace CustomerEngagement.Enterprise.Saml.Services;

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
