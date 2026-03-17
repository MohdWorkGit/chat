using System.ComponentModel.DataAnnotations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Enterprise.Saml.Entities;

public class SamlRoleMapping : BaseEntity
{
    public int SamlConfigId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string SamlAttributeValue { get; set; }

    public UserRole UserRole { get; set; }

    // Navigation properties
    public SamlConfig SamlConfig { get; set; } = null!;
}
