using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Entities;

namespace CustomerEngagement.Enterprise.Saml.Entities;

public class SamlConfig : BaseEntity
{
    public int AccountId { get; set; }

    [Required]
    public required string IdpEntityId { get; set; }

    [Required]
    [MaxLength(2048)]
    public required string IdpSsoTargetUrl { get; set; }

    [Required]
    public required string IdpCertificate { get; set; }

    [Required]
    public required string SpEntityId { get; set; }

    [Required]
    [MaxLength(2048)]
    public required string AssertionConsumerServiceUrl { get; set; }

    [MaxLength(255)]
    public string? NameIdentifierFormat { get; set; } = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";

    public bool Enabled { get; set; } = false;

    // Navigation properties
    public Account Account { get; set; } = null!;

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(IdpEntityId)
            && !string.IsNullOrWhiteSpace(IdpSsoTargetUrl)
            && !string.IsNullOrWhiteSpace(IdpCertificate)
            && !string.IsNullOrWhiteSpace(SpEntityId)
            && !string.IsNullOrWhiteSpace(AssertionConsumerServiceUrl);
    }
}
