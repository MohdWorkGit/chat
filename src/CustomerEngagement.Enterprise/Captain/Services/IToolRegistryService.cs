using CustomerEngagement.Enterprise.Captain.Entities;

namespace CustomerEngagement.Enterprise.Captain.Services;

public record ToolExecutionResult(bool Success, string? Output, string? Error = null);

public interface IToolRegistryService
{
    Task<IReadOnlyList<CaptainCustomTool>> GetToolsForAssistantAsync(
        int assistantId,
        CancellationToken cancellationToken = default);

    Task<ToolExecutionResult> ExecuteToolAsync(
        int toolId,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default);

    Task RegisterToolAsync(
        CaptainCustomTool tool,
        CancellationToken cancellationToken = default);

    Task UnregisterToolAsync(
        int toolId,
        CancellationToken cancellationToken = default);
}
