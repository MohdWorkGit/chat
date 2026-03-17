using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Contacts;

public interface IContactService
{
    Task<ContactDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginatedResultDto<ContactDto>> GetByAccountAsync(
        int accountId,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<ContactDto> CreateAsync(int accountId, CreateContactRequest request, CancellationToken cancellationToken = default);

    Task<ContactDto> UpdateAsync(int contactId, UpdateContactRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int contactId, CancellationToken cancellationToken = default);

    Task<PaginatedResultDto<ContactDto>> SearchAsync(
        int accountId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<ContactDto> MergeAsync(int baseContactId, int mergeContactId, CancellationToken cancellationToken = default);
}

public class CreateContactRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
}

public class UpdateContactRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
}
