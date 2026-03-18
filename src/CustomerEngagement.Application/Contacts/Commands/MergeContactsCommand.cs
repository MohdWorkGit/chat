using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using MediatR;

namespace CustomerEngagement.Application.Contacts.Commands;

public record MergeContactsCommand(
    long AccountId,
    int BaseContactId,
    int MergeContactId) : IRequest<ContactDto>;

public class MergeContactsCommandHandler : IRequestHandler<MergeContactsCommand, ContactDto>
{
    private readonly IContactService _contactService;

    public MergeContactsCommandHandler(IContactService contactService)
    {
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
    }

    public async Task<ContactDto> Handle(MergeContactsCommand request, CancellationToken cancellationToken)
    {
        return await _contactService.MergeAsync(request.BaseContactId, request.MergeContactId, cancellationToken);
    }
}
