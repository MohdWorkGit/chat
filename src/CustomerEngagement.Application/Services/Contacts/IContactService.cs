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
