using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Channels;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Inboxes.Commands;

public record CreateInboxCommand(long AccountId = 0, string Name = "", string? ChannelType = null, int? ChannelId = null) : IRequest<long>;

public class CreateInboxCommandHandler : IRequestHandler<CreateInboxCommand, long>
{
    private readonly IRepository<Inbox> _inboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebWidgetService _webWidgetService;

    public CreateInboxCommandHandler(
        IRepository<Inbox> inboxRepository,
        IUnitOfWork unitOfWork,
        IWebWidgetService webWidgetService)
    {
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _webWidgetService = webWidgetService ?? throw new ArgumentNullException(nameof(webWidgetService));
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

        if (request.ChannelType == "web_widget")
        {
            await _webWidgetService.CreateWidgetAsync(
                (int)request.AccountId,
                new CreateWebWidgetRequest { InboxId = inbox.Id },
                cancellationToken);
        }

        return inbox.Id;
    }
}

public record UpdateInboxCommand(long AccountId = 0, long Id = 0, string? Name = null,
    string? GreetingMessage = null, bool? EnableAutoAssignment = null, bool? CsatSurveyEnabled = null) : IRequest<object>;

public class UpdateInboxCommandHandler : IRequestHandler<UpdateInboxCommand, object>
{
    private readonly IRepository<Inbox> _inboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInboxCommandHandler(IRepository<Inbox> inboxRepository, IUnitOfWork unitOfWork)
    {
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(UpdateInboxCommand request, CancellationToken cancellationToken)
    {
        var inboxes = await _inboxRepository.FindAsync(
            i => i.AccountId == (int)request.AccountId && i.Id == (int)request.Id,
            cancellationToken);

        var inbox = inboxes.FirstOrDefault();
        if (inbox is null)
            return new { Error = "Inbox not found" };

        if (request.Name is not null) inbox.Name = request.Name;
        if (request.GreetingMessage is not null) inbox.GreetingMessage = request.GreetingMessage;
        if (request.EnableAutoAssignment.HasValue) inbox.EnableAutoAssignment = request.EnableAutoAssignment.Value;
        if (request.CsatSurveyEnabled.HasValue) inbox.CsatSurveyEnabled = request.CsatSurveyEnabled.Value;
        inbox.UpdatedAt = DateTime.UtcNow;

        _inboxRepository.Update(inbox);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { inbox.Id };
    }
}

public record DeleteInboxCommand(long AccountId, long Id) : IRequest<object>;

public class DeleteInboxCommandHandler : IRequestHandler<DeleteInboxCommand, object>
{
    private readonly IRepository<Inbox> _inboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInboxCommandHandler(IRepository<Inbox> inboxRepository, IUnitOfWork unitOfWork)
    {
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(DeleteInboxCommand request, CancellationToken cancellationToken)
    {
        var inboxes = await _inboxRepository.FindAsync(
            i => i.AccountId == (int)request.AccountId && i.Id == (int)request.Id,
            cancellationToken);

        var inbox = inboxes.FirstOrDefault();
        if (inbox is null)
            return new { Error = "Inbox not found" };

        _inboxRepository.Remove(inbox);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { Success = true };
    }
}

public record AddInboxMemberCommand(long AccountId = 0, long InboxId = 0, long UserId = 0) : IRequest<object>;

public class AddInboxMemberCommandHandler : IRequestHandler<AddInboxMemberCommand, object>
{
    private readonly IRepository<InboxMember> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddInboxMemberCommandHandler(IRepository<InboxMember> memberRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(AddInboxMemberCommand request, CancellationToken cancellationToken)
    {
        var existing = await _memberRepository.FindAsync(
            m => m.InboxId == (int)request.InboxId && m.UserId == (int)request.UserId,
            cancellationToken);

        if (existing.Any())
            return new { Error = "Member already exists" };

        var member = new InboxMember
        {
            InboxId = (int)request.InboxId,
            UserId = (int)request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _memberRepository.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { member.Id, member.InboxId, member.UserId };
    }
}

public record RemoveInboxMemberCommand(long AccountId, long InboxId, long UserId) : IRequest<object>;

public class RemoveInboxMemberCommandHandler : IRequestHandler<RemoveInboxMemberCommand, object>
{
    private readonly IRepository<InboxMember> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveInboxMemberCommandHandler(IRepository<InboxMember> memberRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(RemoveInboxMemberCommand request, CancellationToken cancellationToken)
    {
        var members = await _memberRepository.FindAsync(
            m => m.InboxId == (int)request.InboxId && m.UserId == (int)request.UserId,
            cancellationToken);

        var member = members.FirstOrDefault();
        if (member is null)
            return new { Error = "Member not found" };

        _memberRepository.Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { Success = true };
    }
}

public record UpdateWorkingHoursCommand(long AccountId = 0, long InboxId = 0) : IRequest<object>;
