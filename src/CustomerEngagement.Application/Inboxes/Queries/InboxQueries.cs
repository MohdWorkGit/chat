using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Channels;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Entities.Channels;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Inboxes.Queries;

public record GetInboxesQuery(long AccountId) : IRequest<object>;

public record GetInboxByIdQuery(long AccountId, long Id) : IRequest<object>;

public record GetInboxMembersQuery(long AccountId, long InboxId) : IRequest<object>;

public record GetWorkingHoursQuery(long AccountId, long InboxId) : IRequest<object>;

public record GetInboxWidgetConfigQuery(long AccountId, long InboxId) : IRequest<object>;

public class GetInboxesQueryHandler : IRequestHandler<GetInboxesQuery, object>
{
    private readonly IRepository<Inbox> _inboxRepository;

    public GetInboxesQueryHandler(IRepository<Inbox> inboxRepository)
    {
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
    }

    public async Task<object> Handle(GetInboxesQuery request, CancellationToken cancellationToken)
    {
        var inboxes = await _inboxRepository.FindAsync(
            i => i.AccountId == (int)request.AccountId, cancellationToken);

        return inboxes.Select(i => new
        {
            i.Id,
            i.AccountId,
            i.Name,
            i.ChannelType,
            i.GreetingEnabled,
            i.GreetingMessage,
            i.CreatedAt
        }).ToList();
    }
}

public class GetInboxByIdQueryHandler : IRequestHandler<GetInboxByIdQuery, object>
{
    private readonly IRepository<Inbox> _inboxRepository;

    public GetInboxByIdQueryHandler(IRepository<Inbox> inboxRepository)
    {
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
    }

    public async Task<object> Handle(GetInboxByIdQuery request, CancellationToken cancellationToken)
    {
        var inboxes = await _inboxRepository.FindAsync(
            i => i.AccountId == (int)request.AccountId && i.Id == (int)request.Id,
            cancellationToken);

        var inbox = inboxes.FirstOrDefault();

        if (inbox is null)
            return new { Error = "Inbox not found" };

        return new
        {
            inbox.Id,
            inbox.AccountId,
            inbox.Name,
            inbox.ChannelType,
            inbox.GreetingEnabled,
            inbox.GreetingMessage,
            inbox.CreatedAt
        };
    }
}

public class GetInboxMembersQueryHandler : IRequestHandler<GetInboxMembersQuery, object>
{
    private readonly IRepository<InboxMember> _inboxMemberRepository;

    public GetInboxMembersQueryHandler(IRepository<InboxMember> inboxMemberRepository)
    {
        _inboxMemberRepository = inboxMemberRepository ?? throw new ArgumentNullException(nameof(inboxMemberRepository));
    }

    public async Task<object> Handle(GetInboxMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _inboxMemberRepository.FindAsync(
            m => m.InboxId == (int)request.InboxId, cancellationToken);

        return members.Select(m => new
        {
            m.Id,
            m.InboxId,
            m.UserId,
            m.CreatedAt
        }).ToList();
    }
}

public class GetWorkingHoursQueryHandler : IRequestHandler<GetWorkingHoursQuery, object>
{
    private readonly IRepository<WorkingHour> _workingHourRepository;

    public GetWorkingHoursQueryHandler(IRepository<WorkingHour> workingHourRepository)
    {
        _workingHourRepository = workingHourRepository ?? throw new ArgumentNullException(nameof(workingHourRepository));
    }

    public async Task<object> Handle(GetWorkingHoursQuery request, CancellationToken cancellationToken)
    {
        var hours = await _workingHourRepository.FindAsync(
            h => h.InboxId == (int)request.InboxId && h.AccountId == (int)request.AccountId,
            cancellationToken);

        return hours.Select(h => new
        {
            h.Id,
            h.InboxId,
            h.AccountId,
            h.DayOfWeek,
            h.OpenHour,
            h.OpenMinutes,
            h.CloseHour,
            h.CloseMinutes,
            h.ClosedAllDay,
            h.OpenAllDay
        }).ToList();
    }
}

public class GetInboxWidgetConfigQueryHandler : IRequestHandler<GetInboxWidgetConfigQuery, object>
{
    private readonly IRepository<Inbox> _inboxRepository;
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IWebWidgetService _webWidgetService;

    public GetInboxWidgetConfigQueryHandler(
        IRepository<Inbox> inboxRepository,
        IRepository<ChannelWebWidget> widgetRepository,
        IWebWidgetService webWidgetService)
    {
        _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
        _widgetRepository = widgetRepository ?? throw new ArgumentNullException(nameof(widgetRepository));
        _webWidgetService = webWidgetService ?? throw new ArgumentNullException(nameof(webWidgetService));
    }

    public async Task<object> Handle(GetInboxWidgetConfigQuery request, CancellationToken cancellationToken)
    {
        var inboxes = await _inboxRepository.FindAsync(
            i => i.AccountId == (int)request.AccountId && i.Id == (int)request.InboxId,
            cancellationToken);

        var inbox = inboxes.FirstOrDefault();
        if (inbox is null)
            return new { Error = "Inbox not found" };

        var widgets = await _widgetRepository.FindAsync(
            w => w.InboxId == (int)request.InboxId && w.AccountId == (int)request.AccountId,
            cancellationToken);

        var widget = widgets.FirstOrDefault();

        // Auto-create widget for existing web_widget inboxes that are missing the record
        if (widget is null && inbox.ChannelType == "web_widget")
        {
            var dto = await _webWidgetService.CreateWidgetAsync(
                (int)request.AccountId,
                new CreateWebWidgetRequest { InboxId = (int)request.InboxId },
                cancellationToken);

            return new
            {
                dto.Id,
                InboxId = (int)request.InboxId,
                dto.AccountId,
                WebsiteToken = dto.Token,
                dto.WebsiteUrl,
                dto.WelcomeTitle,
                dto.WelcomeTagline,
                dto.WidgetColor,
                dto.IsEnabled,
                dto.PreChatFormEnabled,
                dto.PreChatFormOptions,
                dto.CreatedAt
            };
        }

        if (widget is null)
            return new { Error = "No web widget configuration found for this inbox" };

        return new
        {
            widget.Id,
            widget.InboxId,
            widget.AccountId,
            widget.WebsiteToken,
            widget.WebsiteUrl,
            widget.WelcomeTitle,
            widget.WelcomeTagline,
            widget.WidgetColor,
            widget.IsEnabled,
            widget.PreChatFormEnabled,
            widget.PreChatFormOptions,
            widget.CreatedAt
        };
    }
}
