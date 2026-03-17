using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CustomerEngagement.Core.Enums;

namespace CustomerEngagement.Core.Entities;

public class Message : BaseEntity
{
    public int ConversationId { get; set; }
    public int AccountId { get; set; }
    public int? SenderId { get; set; }

    [MaxLength(50)]
    public string? SenderType { get; set; }

    public string? Content { get; set; }

    [MaxLength(50)]
    public string? ContentType { get; set; }

    public MessageType MessageType { get; set; } = MessageType.Incoming;
    public bool Private { get; set; }
    public int Status { get; set; }

    [JsonPropertyName("external_source_ids")]
    public string? ExternalSourceIds { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public ICollection<Attachment> Attachments { get; set; } = [];
}
