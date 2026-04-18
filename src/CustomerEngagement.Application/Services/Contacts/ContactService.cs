using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Services.Contacts;

public class ContactService : IContactService
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IContactMergeService _contactMergeService;

    public ContactService(
        IRepository<Contact> contactRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IContactMergeService contactMergeService)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _contactMergeService = contactMergeService ?? throw new ArgumentNullException(nameof(contactMergeService));
    }

    public async Task<ContactDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken);
        return contact is null ? null : MapToDto(contact);
    }

    public async Task<PaginatedResultDto<ContactDto>> GetByAccountAsync(
        int accountId,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var contacts = await _contactRepository.ListAsync(new { AccountId = accountId }, cancellationToken);
        var totalCount = contacts.Count;

        var items = contacts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PaginatedResultDto<ContactDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ContactDto> CreateAsync(int accountId, CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        var contact = new Contact
        {
            AccountId = accountId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CompanyName = request.CompanyName,
            Location = request.Location,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new ContactCreatedEvent(contact.Id, accountId), cancellationToken);

        return MapToDto(contact);
    }

    public async Task<ContactDto> UpdateAsync(int contactId, UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId, cancellationToken)
            ?? throw new InvalidOperationException($"Contact {contactId} not found.");

        if (request.Name is not null) contact.Name = request.Name;
        if (request.Email is not null) contact.Email = request.Email;
        if (request.Phone is not null) contact.Phone = request.Phone;
        if (request.CompanyName is not null) contact.CompanyName = request.CompanyName;
        if (request.Location is not null) contact.Location = request.Location;
        contact.UpdatedAt = DateTime.UtcNow;

        await _contactRepository.UpdateAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(contact);
    }

    public async Task DeleteAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId, cancellationToken)
            ?? throw new InvalidOperationException($"Contact {contactId} not found.");

        await _contactRepository.DeleteAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedResultDto<ContactDto>> SearchAsync(
        int accountId,
        string query,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var contacts = await _contactRepository.ListAsync(
            new { AccountId = accountId, SearchQuery = query },
            cancellationToken);

        var totalCount = contacts.Count();

        var items = contacts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PaginatedResultDto<ContactDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ContactDto> MergeAsync(int baseContactId, int mergeContactId, CancellationToken cancellationToken = default)
    {
        var mergedContact = await _contactMergeService.MergeContactsAsync(baseContactId, mergeContactId, cancellationToken);
        return mergedContact;
    }

    private static ContactDto MapToDto(Contact contact)
    {
        return new ContactDto(
            contact.Id,
            contact.AccountId,
            contact.Name,
            contact.Email,
            contact.Phone,
            contact.Identifier,
            contact.ContactType.ToString(),
            contact.CompanyName,
            contact.Location,
            contact.CountryCode,
            contact.LastActivityAt,
            contact.CreatedAt,
            contact.UpdatedAt,
            null,
            0);
    }
}
