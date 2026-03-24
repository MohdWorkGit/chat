using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using MediatR;

namespace CustomerEngagement.Application.Contacts.Commands;

public record UpdateContactCommand(
    long AccountId,
    long Id,
    string? Name,
    string? Email,
    string? Phone) : IRequest;

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand>
{
    private readonly IContactService _contactService;

    public UpdateContactCommandHandler(IContactService contactService)
    {
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
    }

    public async Task Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateContactRequest(
            request.Name, request.Email, request.Phone,
            null, null, null, null);

        await _contactService.UpdateAsync((int)request.Id, updateRequest, cancellationToken);
    }
}
