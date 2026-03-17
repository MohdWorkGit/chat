using System.Text;
using System.Xml.Linq;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using CustomerEngagement.Enterprise.Saml.Entities;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.Saml.Services;

public class SamlAuthService : ISamlAuthService
{
    private static readonly XNamespace SamlProtocol = "urn:oasis:names:tc:SAML:2.0:protocol";
    private static readonly XNamespace SamlAssertion = "urn:oasis:names:tc:SAML:2.0:assertion";

    private readonly IRepository<SamlConfig> _configRepository;
    private readonly IRepository<SamlRoleMapping> _roleMappingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SamlAuthService> _logger;

    public SamlAuthService(
        IRepository<SamlConfig> configRepository,
        IRepository<SamlRoleMapping> roleMappingRepository,
        IUnitOfWork unitOfWork,
        ILogger<SamlAuthService> logger)
    {
        _configRepository = configRepository;
        _roleMappingRepository = roleMappingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SamlConfigDto?> GetConfigAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.FindAsync(c => c.AccountId == accountId, cancellationToken);
        var config = configs.FirstOrDefault();
        return config is null ? null : MapToDto(config);
    }

    public async Task<SamlConfigDto> CreateOrUpdateConfigAsync(int accountId, SamlConfigDto config, CancellationToken cancellationToken = default)
    {
        var existing = await _configRepository.FindAsync(c => c.AccountId == accountId, cancellationToken);
        var entity = existing.FirstOrDefault();

        if (entity is null)
        {
            entity = new SamlConfig
            {
                AccountId = accountId,
                IdpEntityId = config.IdpEntityId,
                IdpSsoTargetUrl = config.IdpSsoTargetUrl,
                IdpCertificate = config.IdpCertificate,
                SpEntityId = config.SpEntityId,
                AssertionConsumerServiceUrl = config.AssertionConsumerServiceUrl,
                NameIdentifierFormat = config.NameIdentifierFormat,
                Enabled = config.Enabled
            };

            entity = await _configRepository.AddAsync(entity, cancellationToken);
            _logger.LogInformation("Created SAML config for account {AccountId}", accountId);
        }
        else
        {
            entity.IdpEntityId = config.IdpEntityId;
            entity.IdpSsoTargetUrl = config.IdpSsoTargetUrl;
            entity.IdpCertificate = config.IdpCertificate;
            entity.SpEntityId = config.SpEntityId;
            entity.AssertionConsumerServiceUrl = config.AssertionConsumerServiceUrl;
            entity.NameIdentifierFormat = config.NameIdentifierFormat;
            entity.Enabled = config.Enabled;
            entity.UpdatedAt = DateTime.UtcNow;

            await _configRepository.UpdateAsync(entity, cancellationToken);
            _logger.LogInformation("Updated SAML config for account {AccountId}", accountId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<SamlValidationResult> ValidateAssertionAsync(int accountId, string samlResponse, CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.FindAsync(c => c.AccountId == accountId, cancellationToken);
        var config = configs.FirstOrDefault()
            ?? throw new InvalidOperationException($"SAML configuration not found for account {accountId}.");

        if (!config.Enabled)
        {
            throw new InvalidOperationException("SAML SSO is not enabled for this account.");
        }

        // Decode base64 SAML response
        var decodedBytes = Convert.FromBase64String(samlResponse);
        var decodedXml = Encoding.UTF8.GetString(decodedBytes);

        var doc = XDocument.Parse(decodedXml);

        // Extract NameID
        var nameId = doc.Descendants(SamlAssertion + "NameID").FirstOrDefault()?.Value
            ?? throw new InvalidOperationException("NameID not found in SAML assertion.");

        // Extract attributes
        var attributes = new Dictionary<string, string>();
        var attributeStatements = doc.Descendants(SamlAssertion + "AttributeStatement");
        foreach (var statement in attributeStatements)
        {
            foreach (var attribute in statement.Elements(SamlAssertion + "Attribute"))
            {
                var name = attribute.Attribute("Name")?.Value;
                var value = attribute.Elements(SamlAssertion + "AttributeValue").FirstOrDefault()?.Value;
                if (name is not null && value is not null)
                {
                    attributes[name] = value;
                }
            }
        }

        // Determine email from NameID or attributes
        var email = nameId;
        if (attributes.TryGetValue("email", out var emailAttr))
        {
            email = emailAttr;
        }

        // Map role via SamlRoleMapping
        UserRole? mappedRole = null;
        var roleMappings = await _roleMappingRepository.FindAsync(
            m => m.SamlConfigId == config.Id, cancellationToken);

        foreach (var mapping in roleMappings)
        {
            if (attributes.Any(a => string.Equals(a.Value, mapping.SamlAttributeValue, StringComparison.OrdinalIgnoreCase)))
            {
                mappedRole = mapping.UserRole;
                break;
            }
        }

        _logger.LogInformation(
            "Validated SAML assertion for account {AccountId}: NameID={NameId}, Email={Email}, MappedRole={MappedRole}",
            accountId, nameId, email, mappedRole);

        return new SamlValidationResult(email, nameId, attributes, mappedRole);
    }

    public async Task<IReadOnlyList<SamlRoleMappingDto>> GetRoleMappingsAsync(int samlConfigId, CancellationToken cancellationToken = default)
    {
        var mappings = await _roleMappingRepository.FindAsync(m => m.SamlConfigId == samlConfigId, cancellationToken);
        return mappings.Select(MapMappingToDto).ToList();
    }

    public async Task<SamlRoleMappingDto> CreateRoleMappingAsync(int samlConfigId, string samlAttributeValue, UserRole userRole, CancellationToken cancellationToken = default)
    {
        var mapping = new SamlRoleMapping
        {
            SamlConfigId = samlConfigId,
            SamlAttributeValue = samlAttributeValue,
            UserRole = userRole
        };

        var created = await _roleMappingRepository.AddAsync(mapping, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created SAML role mapping: ConfigId={ConfigId}, AttributeValue={AttributeValue}, Role={Role}",
            samlConfigId, samlAttributeValue, userRole);

        return MapMappingToDto(created);
    }

    public async Task DeleteRoleMappingAsync(int roleMappingId, CancellationToken cancellationToken = default)
    {
        var mapping = await _roleMappingRepository.GetByIdAsync(roleMappingId, cancellationToken)
            ?? throw new InvalidOperationException($"SAML role mapping with id {roleMappingId} not found.");

        await _roleMappingRepository.DeleteAsync(mapping, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted SAML role mapping {MappingId}", roleMappingId);
    }

    public async Task<string> GenerateSpMetadataXmlAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.FindAsync(c => c.AccountId == accountId, cancellationToken);
        var config = configs.FirstOrDefault()
            ?? throw new InvalidOperationException($"SAML configuration not found for account {accountId}.");

        XNamespace md = "urn:oasis:names:tc:SAML:2.0:metadata";

        var metadata = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(md + "EntityDescriptor",
                new XAttribute("entityID", config.SpEntityId),
                new XAttribute(XNamespace.Xmlns + "md", md.NamespaceName),
                new XElement(md + "SPSSODescriptor",
                    new XAttribute("protocolSupportEnumeration", "urn:oasis:names:tc:SAML:2.0:protocol"),
                    new XAttribute("AuthnRequestsSigned", "false"),
                    new XAttribute("WantAssertionsSigned", "true"),
                    new XElement(md + "NameIDFormat",
                        config.NameIdentifierFormat ?? "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress"),
                    new XElement(md + "AssertionConsumerService",
                        new XAttribute("Binding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"),
                        new XAttribute("Location", config.AssertionConsumerServiceUrl),
                        new XAttribute("index", "1")))));

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            metadata.Save(writer);
        }

        return sb.ToString();
    }

    private static SamlConfigDto MapToDto(SamlConfig config)
    {
        return new SamlConfigDto(
            config.Id,
            config.AccountId,
            config.IdpEntityId,
            config.IdpSsoTargetUrl,
            config.IdpCertificate,
            config.SpEntityId,
            config.AssertionConsumerServiceUrl,
            config.NameIdentifierFormat,
            config.Enabled,
            config.CreatedAt,
            config.UpdatedAt);
    }

    private static SamlRoleMappingDto MapMappingToDto(SamlRoleMapping mapping)
    {
        return new SamlRoleMappingDto(
            mapping.Id,
            mapping.SamlConfigId,
            mapping.SamlAttributeValue,
            mapping.UserRole);
    }
}
