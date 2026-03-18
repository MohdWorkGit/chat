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
        var updateRequest = new Services.Contacts.UpdateContactRequest
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone
        };

        await _contactService.UpdateAsync((int)request.Id, updateRequest, cancellationToken);
    }
}
