using CustomerEngagement.Application.Services.Contacts;
using MediatR;

namespace CustomerEngagement.Application.Contacts.Commands;

public record DeleteContactCommand(long AccountId, long Id) : IRequest;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand>
{
    private readonly IContactService _contactService;

    public DeleteContactCommandHandler(IContactService contactService)
    {
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
    }

    public async Task Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        await _contactService.DeleteAsync((int)request.Id, cancellationToken);
    }
}
