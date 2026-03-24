using MediatR;

namespace CustomerEngagement.Application.Inboxes.Queries;

public record GetInboxesQuery(long AccountId) : IRequest<object>;

public record GetInboxByIdQuery(long AccountId, long Id) : IRequest<object>;

public record GetInboxMembersQuery(long AccountId, long InboxId) : IRequest<object>;

public record GetWorkingHoursQuery(long AccountId, long InboxId) : IRequest<object>;
