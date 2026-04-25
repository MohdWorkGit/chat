using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Entities.Channels;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerEngagement.Application.Widget.Queries;

public record GetWidgetConfigQuery(string WebsiteToken) : IRequest<object>;

public record GetWidgetConversationsQuery(string WidgetToken, string ContactIdentifier) : IRequest<object>;

public record GetWidgetMessagesQuery(string WidgetToken, long ConversationId, int Page, int PageSize) : IRequest<object>;

public class GetWidgetConfigQueryHandler : IRequestHandler<GetWidgetConfigQuery, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<Inbox> _inboxRepository;

    public GetWidgetConfigQueryHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<Inbox> inboxRepository)
    {
        _widgetRepository = widgetRepository;
        _inboxRepository = inboxRepository;
    }

    public async Task<object> Handle(GetWidgetConfigQuery request, CancellationToken cancellationToken)
    {
        var widgets = await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WebsiteToken, cancellationToken);

        var widget = widgets.FirstOrDefault();
        if (widget is null)
            return null!;

        var inbox = await _inboxRepository.GetByIdAsync(widget.InboxId, cancellationToken);

        return new
        {
            widget.WebsiteToken,
            widget.WebsiteUrl,
            widget.WelcomeTitle,
            widget.WelcomeTagline,
            widget.WidgetColor,
            widget.ReplyTime,
            widget.PreChatFormEnabled,
            widget.PreChatFormOptions,
            widget.IsEnabled,
            Inbox = inbox is null ? null : new
            {
                inbox.Id,
                inbox.Name,
                inbox.GreetingEnabled,
                inbox.GreetingMessage,
                inbox.EnableEmailCollect
            }
        };
    }
}

public class GetWidgetConversationsQueryHandler : IRequestHandler<GetWidgetConversationsQuery, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<ContactInbox> _contactInboxRepository;
    private readonly IRepository<Conversation> _conversationRepository;

    public GetWidgetConversationsQueryHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<ContactInbox> contactInboxRepository,
        IRepository<Conversation> conversationRepository)
    {
        _widgetRepository = widgetRepository;
        _contactInboxRepository = contactInboxRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task<object> Handle(GetWidgetConversationsQuery request, CancellationToken cancellationToken)
    {
        var widgets = await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WidgetToken, cancellationToken);

        var widget = widgets.FirstOrDefault();
        if (widget is null)
            return new { Data = Array.Empty<object>() };

        var contactInboxes = await _contactInboxRepository.FindAsync(
            ci => ci.InboxId == widget.InboxId && ci.SourceId == request.ContactIdentifier, cancellationToken);

        var contactInbox = contactInboxes.FirstOrDefault();
        if (contactInbox is null)
            return new { Data = Array.Empty<object>() };

        var conversations = await _conversationRepository.FindAsync(
            c => c.InboxId == widget.InboxId && c.ContactId == contactInbox.ContactId, cancellationToken);

        return new
        {
            Data = conversations.Select(c => new
            {
                c.Id,
                c.InboxId,
                c.ContactId,
                c.Status,
                c.CreatedAt,
                c.UpdatedAt
            })
        };
    }
}

public class GetWidgetMessagesQueryHandler : IRequestHandler<GetWidgetMessagesQuery, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IStorageService _storageService;

    public GetWidgetMessagesQueryHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<Message> messageRepository,
        IStorageService storageService)
    {
        _widgetRepository = widgetRepository;
        _messageRepository = messageRepository;
        _storageService = storageService;
    }

    public async Task<object> Handle(GetWidgetMessagesQuery request, CancellationToken cancellationToken)
    {
        var widgets = await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WidgetToken, cancellationToken);

        var widget = widgets.FirstOrDefault();
        if (widget is null)
            return new { Data = Array.Empty<object>(), Meta = new { TotalCount = 0, Page = request.Page, PageSize = request.PageSize, TotalPages = 0 } };

        var conversationId = (int)request.ConversationId;

        // Private messages are internal agent notes and must never be exposed
        // to the widget client (embedded on an end-user's browser).
        var baseQuery = _messageRepository.QueryNoTracking()
            .Where(m => m.ConversationId == conversationId && !m.Private);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var messages = await baseQuery
            .Include(m => m.Attachments)
            .OrderBy(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new
        {
            Data = messages.Select(m => new
            {
                m.Id,
                m.ConversationId,
                m.Content,
                m.ContentType,
                m.MessageType,
                m.SenderType,
                m.CreatedAt,
                Attachments = m.Attachments.Select(a => new
                {
                    a.Id,
                    a.MessageId,
                    FileType = a.FileType.ToString().ToLowerInvariant(),
                    a.FileName,
                    FileUrl = _storageService.GetFileUrl(a.ExternalUrl),
                    a.FileSize,
                    a.ContentType,
                    ThumbUrl = _storageService.GetFileUrl(a.ThumbnailUrl),
                    a.Extension,
                    a.CreatedAt
                }).ToList()
            }),
            Meta = new
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        };
    }
}
