namespace CustomerEngagement.Application.DTOs;

public record InboxDto(
    int Id, int AccountId, string Name, string? ChannelType,
    bool GreetingEnabled, string? GreetingMessage,
    bool EnableAutoAssignment, bool WorkingHoursEnabled,
    bool CsatSurveyEnabled, string? OutOfOfficeMessage,
    DateTime CreatedAt);

public record InboxSummaryDto(int Id, string Name, string? ChannelType);

public record CreateInboxRequest(
    string Name, string ChannelType, bool? GreetingEnabled,
    string? GreetingMessage, bool? EnableAutoAssignment);

public record UpdateInboxRequest(
    string? Name, bool? GreetingEnabled, string? GreetingMessage,
    bool? EnableAutoAssignment, bool? WorkingHoursEnabled,
    bool? CsatSurveyEnabled, string? OutOfOfficeMessage);
