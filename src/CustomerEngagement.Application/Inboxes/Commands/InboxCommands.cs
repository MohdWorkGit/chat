using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Inboxes.Commands;

public record CreateInboxCommand(long AccountId = 0, string Name = "", string? ChannelType = null, int? ChannelId = null) : IRequest<long>;

public class CreateInboxCommandHandler : IRequestHandler<CreateInboxCommand, long>
{
    private readonly IRepository<Inbox> _inboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInboxCommandHandler(IRepository<Inbox> inboxRepository, IUnitOfWork unitOfWork)
    {
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<long> Handle(CreateInboxCommand request, CancellationToken cancellationToken)
    {
        var inbox = new Inbox
        {
            AccountId = (int)request.AccountId,
            Name = request.Name,
            ChannelType = request.ChannelType,
            ChannelId = request.ChannelId ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _inboxRepository.AddAsync(inbox, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return inbox.Id;
    }
}

public record UpdateInboxCommand(long AccountId = 0, long Id = 0, string? Name = null) : IRequest<object>;

public record DeleteInboxCommand(long AccountId, long Id) : IRequest<object>;

public record AddInboxMemberCommand(long AccountId = 0, long InboxId = 0, long UserId = 0) : IRequest<object>;

public record RemoveInboxMemberCommand(long AccountId, long InboxId, long UserId) : IRequest<object>;

public record UpdateWorkingHoursCommand(long AccountId = 0, long InboxId = 0) : IRequest<object>;
