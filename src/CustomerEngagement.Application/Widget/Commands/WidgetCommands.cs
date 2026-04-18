using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Application.Hubs;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Entities.Channels;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CustomerEngagement.Application.Widget.Commands;

public record CreateWidgetContactCommand(string WidgetToken = "", string? Name = null, string? Email = null) : IRequest<object>;

public record UpdateWidgetContactCommand(string WidgetToken = "", string ContactIdentifier = "", string? Name = null, string? Email = null) : IRequest<object>;

public record CreateWidgetConversationCommand(
    string WidgetToken = "",
    string? Name = null,
    string? Email = null,
    Dictionary<string, string>? CustomFields = null) : IRequest<object>;

public record SendWidgetMessageCommand(string WidgetToken = "", long ConversationId = 0, string? Content = null) : IRequest<object>;

public record SubmitWidgetCsatCommand(
    string WidgetToken = "",
    long ConversationId = 0,
    int Rating = 0,
    string? Feedback = null) : IRequest<object>;

public record UploadWidgetAttachmentCommand(
    string WidgetToken = "",
    long ConversationId = 0,
    string FileName = "",
    string ContentType = "",
    byte[] FileBytes = default!) : IRequest<object>;

// ---------------------------------------------------------------------------
// Command Handlers
// ---------------------------------------------------------------------------

public class CreateWidgetContactCommandHandler : IRequestHandler<CreateWidgetContactCommand, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<ContactInbox> _contactInboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWidgetContactCommandHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<Contact> contactRepository,
        IRepository<ContactInbox> contactInboxRepository,
        IUnitOfWork unitOfWork)
    {
        _widgetRepository = widgetRepository;
        _contactRepository = contactRepository;
        _contactInboxRepository = contactInboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(CreateWidgetContactCommand request, CancellationToken cancellationToken)
    {
        var widget = (await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WidgetToken, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Invalid widget token.");

        var contact = new Contact
        {
            AccountId = widget.AccountId,
            Name = request.Name,
            Email = request.Email,
            ContactType = ContactType.Visitor,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sourceId = Guid.NewGuid().ToString();
        var contactInbox = new ContactInbox
        {
            ContactId = contact.Id,
            InboxId = widget.InboxId,
            SourceId = sourceId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contactInboxRepository.AddAsync(contactInbox, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { contact.Id, contact.Name, contact.Email, SourceId = sourceId };
    }
}

public class UpdateWidgetContactCommandHandler : IRequestHandler<UpdateWidgetContactCommand, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<ContactInbox> _contactInboxRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWidgetContactCommandHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<ContactInbox> contactInboxRepository,
        IRepository<Contact> contactRepository,
        IUnitOfWork unitOfWork)
    {
        _widgetRepository = widgetRepository;
        _contactInboxRepository = contactInboxRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(UpdateWidgetContactCommand request, CancellationToken cancellationToken)
    {
        var widget = (await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WidgetToken, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Invalid widget token.");

        var contactInbox = (await _contactInboxRepository.FindAsync(
            ci => ci.InboxId == widget.InboxId && ci.SourceId == request.ContactIdentifier, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Contact not found.");

        var contact = await _contactRepository.GetByIdAsync(contactInbox.ContactId, cancellationToken)
            ?? throw new InvalidOperationException("Contact not found.");

        if (request.Name is not null) contact.Name = request.Name;
        if (request.Email is not null) contact.Email = request.Email;
        contact.UpdatedAt = DateTime.UtcNow;

        _contactRepository.Update(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { contact.Id, contact.Name, contact.Email };
    }
}

public class CreateWidgetConversationCommandHandler : IRequestHandler<CreateWidgetConversationCommand, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<ContactInbox> _contactInboxRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWidgetConversationCommandHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<Contact> contactRepository,
        IRepository<ContactInbox> contactInboxRepository,
        IRepository<Conversation> conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _widgetRepository = widgetRepository;
        _contactRepository = contactRepository;
        _contactInboxRepository = contactInboxRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(CreateWidgetConversationCommand request, CancellationToken cancellationToken)
    {
        var widget = (await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WidgetToken, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Invalid widget token.");

        // Create or find the contact
        var contact = new Contact
        {
            AccountId = widget.AccountId,
            Name = request.Name,
            Email = request.Email,
            ContactType = ContactType.Visitor,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var contactInbox = new ContactInbox
        {
            ContactId = contact.Id,
            InboxId = widget.InboxId,
            SourceId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contactInboxRepository.AddAsync(contactInbox, cancellationToken);

        // Create the conversation. DisplayId is account-scoped and must be
        // unique per (AccountId, DisplayId); generate the next value.
        var displayId = await ConversationDisplayIdGenerator.GetNextDisplayIdAsync(
            _conversationRepository, widget.AccountId, cancellationToken);

        var conversation = new Conversation
        {
            AccountId = widget.AccountId,
            InboxId = widget.InboxId,
            ContactId = contact.Id,
            DisplayId = displayId,
            Status = ConversationStatus.Open,
            Uuid = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new
        {
            conversation.Id,
            conversation.Status,
            conversation.InboxId,
            conversation.ContactId,
            Messages = Array.Empty<object>()
        };
    }
}

public class SendWidgetMessageCommandHandler : IRequestHandler<SendWidgetMessageCommand, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public SendWidgetMessageCommandHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _widgetRepository = widgetRepository;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<object> Handle(SendWidgetMessageCommand request, CancellationToken cancellationToken)
    {
        var widget = (await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WidgetToken, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Invalid widget token.");

        var conversation = await _conversationRepository.GetByIdAsync((int)request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException("Conversation not found.");

        var message = new Message
        {
            ConversationId = conversation.Id,
            AccountId = widget.AccountId,
            ContactId = conversation.ContactId,
            Content = request.Content,
            ContentType = "text",
            SenderType = "customer",
            MessageType = MessageType.Incoming,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        conversation.LastActivityAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;
        _conversationRepository.Update(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new MessageCreatedEvent(message.Id, conversation.Id, widget.AccountId),
            cancellationToken);

        return new
        {
            message.Id,
            message.ConversationId,
            message.Content,
            message.SenderType,
            message.ContentType,
            message.CreatedAt
        };
    }
}

public class SubmitWidgetCsatCommandHandler : IRequestHandler<SubmitWidgetCsatCommand, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<CsatSurveyResponse> _csatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<ConversationHub> _hubContext;

    public SubmitWidgetCsatCommandHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<CsatSurveyResponse> csatRepository,
        IUnitOfWork unitOfWork,
        IHubContext<ConversationHub> hubContext)
    {
        _widgetRepository = widgetRepository;
        _conversationRepository = conversationRepository;
        _csatRepository = csatRepository;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    public async Task<object> Handle(SubmitWidgetCsatCommand request, CancellationToken cancellationToken)
    {
        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating must be between 1 and 5.");

        var widget = (await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WidgetToken, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Invalid widget token.");

        var conversation = await _conversationRepository.GetByIdAsync((int)request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException("Conversation not found.");

        var response = new CsatSurveyResponse
        {
            AccountId = widget.AccountId,
            ConversationId = conversation.Id,
            ContactId = conversation.ContactId,
            Rating = request.Rating,
            FeedbackText = request.Feedback,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _csatRepository.AddAsync(response, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients.Group($"account_{widget.AccountId}")
            .SendAsync("CsatReceived", new
            {
                response.Id,
                response.ConversationId,
                response.Rating,
                response.FeedbackText,
                response.CreatedAt
            }, cancellationToken);

        return new { response.Id, response.Rating, response.FeedbackText };
    }
}

public class UploadWidgetAttachmentCommandHandler : IRequestHandler<UploadWidgetAttachmentCommand, object>
{
    private readonly IRepository<ChannelWebWidget> _widgetRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public UploadWidgetAttachmentCommandHandler(
        IRepository<ChannelWebWidget> widgetRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IRepository<Attachment> attachmentRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _widgetRepository = widgetRepository;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _attachmentRepository = attachmentRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<object> Handle(UploadWidgetAttachmentCommand request, CancellationToken cancellationToken)
    {
        var widget = (await _widgetRepository.FindAsync(
            w => w.WebsiteToken == request.WidgetToken, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Invalid widget token.");

        var conversation = await _conversationRepository.GetByIdAsync((int)request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException("Conversation not found.");

        var key = $"attachments/{widget.AccountId}/{Guid.NewGuid()}/{request.FileName}";
        using var stream = new MemoryStream(request.FileBytes);
        await _storageService.UploadFileAsync(key, stream, request.ContentType, cancellationToken);

        var message = new Message
        {
            ConversationId = conversation.Id,
            AccountId = widget.AccountId,
            ContactId = conversation.ContactId,
            Content = request.FileName,
            ContentType = "attachment",
            SenderType = "customer",
            MessageType = MessageType.Incoming,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        var extension = Path.GetExtension(request.FileName).TrimStart('.');
        var attachment = new Attachment
        {
            AccountId = widget.AccountId,
            ExternalUrl = key,
            FallbackTitle = request.FileName,
            Extension = extension,
            FileType = AttachmentType.File,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _attachmentRepository.AddAsync(attachment, cancellationToken);

        conversation.LastActivityAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;
        _conversationRepository.Update(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        attachment.MessageId = message.Id;
        _attachmentRepository.Update(attachment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new MessageCreatedEvent(message.Id, conversation.Id, widget.AccountId),
            cancellationToken);

        return new
        {
            message.Id,
            message.ConversationId,
            message.Content,
            message.SenderType,
            message.ContentType,
            message.CreatedAt
        };
    }
}
