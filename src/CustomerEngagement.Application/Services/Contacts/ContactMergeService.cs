using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Services.Contacts;

public class ContactMergeService : IContactMergeService
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public ContactMergeService(
        IRepository<Contact> contactRepository,
        IRepository<Conversation> conversationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<ContactDto> MergeContactsAsync(int baseContactId, int mergeContactId, CancellationToken cancellationToken = default)
    {
        var baseContact = await _contactRepository.GetByIdAsync(baseContactId, cancellationToken)
            ?? throw new InvalidOperationException($"Base contact {baseContactId} not found.");

        var mergeContact = await _contactRepository.GetByIdAsync(mergeContactId, cancellationToken)
            ?? throw new InvalidOperationException($"Merge contact {mergeContactId} not found.");

        // Fill in missing fields on base contact from merge contact
        baseContact.Name ??= mergeContact.Name;
        baseContact.Email ??= mergeContact.Email;
        baseContact.Phone ??= mergeContact.Phone;
        baseContact.CompanyName ??= mergeContact.CompanyName;
        baseContact.Location ??= mergeContact.Location;
        baseContact.UpdatedAt = DateTime.UtcNow;

        // Reassign all conversations from merge contact to base contact
        var conversations = await _conversationRepository.ListAsync(
            new { ContactId = mergeContactId },
            cancellationToken);

        foreach (var conversation in conversations)
        {
            conversation.ContactId = baseContactId;
            conversation.UpdatedAt = DateTime.UtcNow;
            await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        }

        // Update base contact and delete merged contact
        await _contactRepository.UpdateAsync(baseContact, cancellationToken);
        await _contactRepository.DeleteAsync(mergeContact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new ContactMergedEvent(baseContactId, mergeContactId, baseContact.AccountId),
            cancellationToken);

        return new ContactDto(
            baseContact.Id,
            baseContact.AccountId,
            baseContact.Name,
            baseContact.Email,
            baseContact.Phone,
            baseContact.Identifier,
            baseContact.ContactType.ToString(),
            baseContact.CompanyName,
            baseContact.Location,
            baseContact.CountryCode,
            baseContact.LastActivityAt,
            baseContact.CreatedAt,
            baseContact.UpdatedAt,
            null,
            0);
    }
}

public record ContactMergedEvent(int BaseContactId, int MergedContactId, int AccountId) : INotification;
