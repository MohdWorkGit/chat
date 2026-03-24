using MediatR;

namespace CustomerEngagement.Application.Inboxes.Commands;

public record CreateInboxCommand(long AccountId = 0, string Name = "", string? ChannelType = null) : IRequest<object>;

public record UpdateInboxCommand(long AccountId = 0, long Id = 0, string? Name = null) : IRequest<object>;

public record DeleteInboxCommand(long AccountId, long Id) : IRequest<object>;

public record AddInboxMemberCommand(long AccountId = 0, long InboxId = 0, long UserId = 0) : IRequest<object>;

public record RemoveInboxMemberCommand(long AccountId, long InboxId, long UserId) : IRequest<object>;

public record UpdateWorkingHoursCommand(long AccountId = 0, long InboxId = 0) : IRequest<object>;
