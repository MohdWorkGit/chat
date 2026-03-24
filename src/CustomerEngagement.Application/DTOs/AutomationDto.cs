namespace CustomerEngagement.Application.DTOs;

public class AutomationContext
{
    public int AccountId { get; set; }
    public long? ConversationId { get; set; }
    public long? MessageId { get; set; }
    public int? ContactId { get; set; }
    public string? EventName { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class AutomationRuleDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public List<AutomationConditionDto> Conditions { get; set; } = new();
    public string ConditionOperator { get; set; } = "AND"; // AND or OR
    public List<AutomationActionDto> Actions { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class AutomationConditionDto
{
    public string Attribute { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty; // equals, contains, starts_with, etc.
    public string Value { get; set; } = string.Empty;
}

public class AutomationActionDto
{
    public string ActionType { get; set; } = string.Empty; // assign_agent, assign_team, add_label, send_message, etc.
    public Dictionary<string, object> Parameters { get; set; } = new();
}
