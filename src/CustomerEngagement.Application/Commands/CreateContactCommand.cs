using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using MediatR;

namespace CustomerEngagement.Application.Commands;

public record CreateContactCommand(
    int AccountId,
    string? Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Location,
    Dictionary<string, object>? CustomAttributes) : IRequest<ContactDto>;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ContactDto>
{
    private readonly IContactService _contactService;

    public CreateContactCommandHandler(IContactService contactService)
    {
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
    }

    public async Task<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new Services.Contacts.CreateContactRequest
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Location = request.Location,
            CustomAttributes = request.CustomAttributes
        };

        return await _contactService.CreateAsync(request.AccountId, createRequest, cancellationToken);
    }
}
