using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Contacts;

public interface IContactMergeService
{
    Task<ContactDto> MergeContactsAsync(int baseContactId, int mergeContactId, CancellationToken cancellationToken = default);
}
