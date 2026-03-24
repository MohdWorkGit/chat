namespace CustomerEngagement.Application.DTOs;

public class GlobalSearchResultDto
{
    public List<ConversationSearchResult> Conversations { get; set; } = [];
    public List<ContactSearchResult> Contacts { get; set; } = [];
    public List<MessageSearchResult> Messages { get; set; } = [];
    public int TotalCount { get; set; }

    public static GlobalSearchResultDto Empty => new();
}

public class ConversationSearchResult
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int InboxId { get; set; }
    public int ContactId { get; set; }
    public string? Identifier { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ContactSearchResult
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Identifier { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageSearchResult
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string? Content { get; set; }
    public string? SenderType { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
