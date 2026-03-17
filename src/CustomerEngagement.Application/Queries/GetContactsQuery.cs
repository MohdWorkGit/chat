using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using MediatR;

namespace CustomerEngagement.Application.Queries;

public record GetContactsQuery(
    int AccountId,
    int Page = 1,
    int PageSize = 25) : IRequest<PaginatedResultDto<ContactDto>>;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, PaginatedResultDto<ContactDto>>
{
    private readonly IContactService _contactService;

    public GetContactsQueryHandler(IContactService contactService)
    {
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
    }

    public async Task<PaginatedResultDto<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        return await _contactService.GetByAccountAsync(
            request.AccountId, request.Page, request.PageSize, cancellationToken);
    }
}
