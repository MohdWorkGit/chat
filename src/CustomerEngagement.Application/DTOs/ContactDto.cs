namespace CustomerEngagement.Application.DTOs;

public record ContactDto(
    int Id, int AccountId, string? Name, string? Email,
    string? Phone, string? Identifier, string ContactType,
    string? CompanyName, string? Location, string? CountryCode,
    DateTime? LastActivityAt, DateTime CreatedAt, DateTime UpdatedAt,
    IDictionary<string, object>? CustomAttributes, int ConversationCount);

public record CreateContactRequest(
    string? Name, string? Email, string? Phone,
    string? Identifier, string? ContactType,
    string? CompanyName, IDictionary<string, object>? CustomAttributes);

public record UpdateContactRequest(
    string? Name, string? Email, string? Phone,
    string? CompanyName, string? Identifier,
    IDictionary<string, object>? CustomAttributes);

public record MergeContactsRequest(int BaseContactId, int MergeContactId);

public record ContactSummaryDto(
    int Id, int AccountId, string? Name, string? Email,
    string? Phone, string? Identifier, string ContactType,
    DateTime? LastActivityAt, DateTime CreatedAt);
