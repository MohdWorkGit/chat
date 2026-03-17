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
    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    public string? ContentAttributes { get; set; }

    [MaxLength(255)]
    public string? SourceId { get; set; }

    public DateTime? SentAt { get; set; }

    [JsonPropertyName("external_source_ids")]
    public string? ExternalSourceIds { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public ICollection<Attachment> Attachments { get; set; } = [];

    /// <summary>
    /// True when the message was sent by a contact (incoming).
    /// </summary>
    public bool IsIncoming => MessageType == MessageType.Incoming;

    /// <summary>
    /// True when the message was sent by an agent (outgoing).
    /// </summary>
    public bool IsOutgoing => MessageType == MessageType.Outgoing;

    /// <summary>
    /// Marks the message as delivered. Only valid from Sent status.
    /// </summary>
    public void MarkDelivered()
    {
        if (Status != MessageStatus.Sent)
            throw new InvalidOperationException(
                $"Cannot mark a message as delivered when its status is '{Status}'. Only Sent messages can be marked as delivered.");

        Status = MessageStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the message as read. Only valid from Sent or Delivered status.
    /// </summary>
    public void MarkRead()
    {
        if (Status != MessageStatus.Sent && Status != MessageStatus.Delivered)
            throw new InvalidOperationException(
                $"Cannot mark a message as read when its status is '{Status}'. Only Sent or Delivered messages can be marked as read.");

        Status = MessageStatus.Read;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the message as failed. Only valid from Sent status.
    /// </summary>
    public void MarkFailed()
    {
        if (Status != MessageStatus.Sent)
            throw new InvalidOperationException(
                $"Cannot mark a message as failed when its status is '{Status}'. Only Sent messages can be marked as failed.");

        Status = MessageStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
