namespace CustomerEngagement.Enterprise.Captain.DTOs;

public record ToolExecutionResult(bool Success, string? Output, string? Error = null);
