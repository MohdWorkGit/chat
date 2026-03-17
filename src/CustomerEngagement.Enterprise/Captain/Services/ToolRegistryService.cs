using System.Net.Http.Json;
using System.Text.Json;
using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Enterprise.Captain.Services;

public class ToolRegistryService : IToolRegistryService
{
    private readonly DbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ToolRegistryService> _logger;

    public ToolRegistryService(
        DbContext dbContext,
        HttpClient httpClient,
        ILogger<ToolRegistryService> logger)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CaptainCustomTool>> GetToolsForAssistantAsync(
        int assistantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CaptainCustomTool>()
            .Where(t => t.AssistantId == assistantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ToolExecutionResult> ExecuteToolAsync(
        int toolId,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        var tool = await _dbContext.Set<CaptainCustomTool>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == toolId, cancellationToken);

        if (tool is null)
        {
            return new ToolExecutionResult(false, null, $"Tool with id {toolId} not found.");
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                tool.EndpointUrl,
                parameters,
                cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new ToolExecutionResult(true, content);
            }

            _logger.LogWarning(
                "Tool {ToolName} execution failed with status {StatusCode}: {Content}",
                tool.Name, response.StatusCode, content);

            return new ToolExecutionResult(false, null, $"Tool returned status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {ToolName} at {EndpointUrl}", tool.Name, tool.EndpointUrl);
            return new ToolExecutionResult(false, null, ex.Message);
        }
    }

    public async Task RegisterToolAsync(
        CaptainCustomTool tool,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Set<CaptainCustomTool>().Add(tool);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnregisterToolAsync(
        int toolId,
        CancellationToken cancellationToken = default)
    {
        var tool = await _dbContext.Set<CaptainCustomTool>()
            .FirstOrDefaultAsync(t => t.Id == toolId, cancellationToken);

        if (tool is not null)
        {
            _dbContext.Set<CaptainCustomTool>().Remove(tool);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
