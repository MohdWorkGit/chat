namespace CustomerEngagement.Application.Services.Automations;

public interface IMacroExecutionService
{
    Task<MacroDto?> GetByIdAsync(int macroId, CancellationToken cancellationToken = default);

    Task<IEnumerable<MacroDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    Task<MacroDto> CreateAsync(int accountId, CreateMacroRequest request, CancellationToken cancellationToken = default);

    Task<MacroDto> UpdateAsync(int macroId, UpdateMacroRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int macroId, CancellationToken cancellationToken = default);

    Task ExecuteAsync(int macroId, long conversationId, CancellationToken cancellationToken = default);
}

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
