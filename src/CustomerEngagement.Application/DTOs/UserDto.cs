namespace CustomerEngagement.Application.DTOs;

public record UserDto(
    int Id, string Name, string? DisplayName, string Email,
    string AvailabilityStatus, string? Avatar,
    string? Role, DateTime CreatedAt);

public record UserSummaryDto(int Id, string Name, string? Avatar, string AvailabilityStatus);

public record UpdateUserRequest(string? Name, string? DisplayName, string? Avatar, string? AvailabilityStatus);
