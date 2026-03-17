using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Services.Automations;

public class MacroExecutionService : IMacroExecutionService
{
    private readonly IRepository<Macro> _macroRepository;
    private readonly IAutomationRuleEngine _automationRuleEngine;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MacroExecutionService> _logger;

    public MacroExecutionService(
        IRepository<Macro> macroRepository,
        IAutomationRuleEngine automationRuleEngine,
        IUnitOfWork unitOfWork,
        ILogger<MacroExecutionService> logger)
    {
        _macroRepository = macroRepository ?? throw new ArgumentNullException(nameof(macroRepository));
        _automationRuleEngine = automationRuleEngine ?? throw new ArgumentNullException(nameof(automationRuleEngine));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MacroDto?> GetByIdAsync(int macroId, CancellationToken cancellationToken = default)
    {
        var macro = await _macroRepository.GetByIdAsync(macroId, cancellationToken);
        return macro is null ? null : MapToDto(macro);
    }

    public async Task<IEnumerable<MacroDto>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var macros = await _macroRepository.ListAsync(new { AccountId = accountId }, cancellationToken);
        return macros.Select(MapToDto);
    }

    public async Task<MacroDto> CreateAsync(int accountId, CreateMacroRequest request, CancellationToken cancellationToken = default)
    {
        var macro = new Macro
        {
            AccountId = accountId,
            Name = request.Name,
            Visibility = request.Visibility.ToString(),
            CreatedById = request.CreatedById,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _macroRepository.AddAsync(macro, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(macro);
        dto.Actions = request.Actions;
        return dto;
    }

    public async Task<MacroDto> UpdateAsync(int macroId, UpdateMacroRequest request, CancellationToken cancellationToken = default)
    {
        var macro = await _macroRepository.GetByIdAsync(macroId, cancellationToken)
            ?? throw new InvalidOperationException($"Macro {macroId} not found.");

        if (request.Name is not null) macro.Name = request.Name;
        if (request.Visibility.HasValue) macro.Visibility = request.Visibility.Value.ToString();
        macro.UpdatedAt = DateTime.UtcNow;

        await _macroRepository.UpdateAsync(macro, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(macro);
    }

    public async Task DeleteAsync(int macroId, CancellationToken cancellationToken = default)
    {
        var macro = await _macroRepository.GetByIdAsync(macroId, cancellationToken)
            ?? throw new InvalidOperationException($"Macro {macroId} not found.");

        await _macroRepository.DeleteAsync(macro, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteAsync(int macroId, long conversationId, CancellationToken cancellationToken = default)
    {
        var macro = await _macroRepository.GetByIdAsync(macroId, cancellationToken)
            ?? throw new InvalidOperationException($"Macro {macroId} not found.");

        var macroDto = MapToDto(macro);
        var ruleDto = new AutomationRuleDto
        {
            Id = macro.Id,
            AccountId = macro.AccountId,
            Name = macro.Name,
            Actions = macroDto.Actions
        };

        var context = new AutomationContext
        {
            AccountId = macro.AccountId,
            ConversationId = conversationId
        };

        await _automationRuleEngine.ExecuteActionsAsync(ruleDto, context, cancellationToken);

        _logger.LogInformation("Executed macro {MacroId} on conversation {ConversationId}", macroId, conversationId);
    }

    private static MacroDto MapToDto(Macro macro)
    {
        return new MacroDto
        {
            Id = macro.Id,
            AccountId = macro.AccountId,
            Name = macro.Name,
            Visibility = int.TryParse(macro.Visibility, out var vis) ? vis : 0,
            CreatedById = macro.CreatedById ?? 0,
            CreatedAt = macro.CreatedAt,
            UpdatedAt = macro.UpdatedAt
        };
    }
}
