using CustomerEngagement.Application.DTOs;

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
