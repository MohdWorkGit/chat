namespace CustomerEngagement.Application.DTOs;

public class MacroDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Visibility { get; set; }
    public int CreatedById { get; set; }
    public List<AutomationActionDto> Actions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMacroRequest
{
    public string Name { get; set; } = string.Empty;
    public int Visibility { get; set; }
    public int CreatedById { get; set; }
    public List<AutomationActionDto> Actions { get; set; } = new();
}

public class UpdateMacroRequest
{
    public string? Name { get; set; }
    public int? Visibility { get; set; }
    public List<AutomationActionDto>? Actions { get; set; }
}
