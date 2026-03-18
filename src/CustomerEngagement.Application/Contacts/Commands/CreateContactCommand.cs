using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using MediatR;

namespace CustomerEngagement.Application.Contacts.Commands;

public record CreateContactCommand(
    long AccountId,
    string? Name,
    string? Email,
    string? Phone,
    string? ContactType,
    string? CompanyName,
    IDictionary<string, object>? CustomAttributes) : IRequest<long>;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, long>
{
    private readonly IContactService _contactService;

    public CreateContactCommandHandler(IContactService contactService)
    {
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
    }

    public async Task<long> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new Services.Contacts.CreateContactRequest
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone
        };

        var result = await _contactService.CreateAsync((int)request.AccountId, createRequest, cancellationToken);
        return result.Id;
    }
}
