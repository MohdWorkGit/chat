using CustomerEngagement.Application.BackgroundJobs;
using CustomerEngagement.Application.Common.Attachments;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Messages.Commands;

/// <summary>
/// Agent-side attachment upload. Creates an outgoing message carrying a
/// single attachment stored in object storage. Content is optional — if
/// the agent types a caption it rides along with the file.
/// </summary>
public record UploadMessageAttachmentCommand(
    long AccountId,
    long ConversationId,
    int? SenderId,
    string? Caption,
    bool IsPrivate,
    string FileName,
    string ContentType,
    byte[] FileBytes) : IRequest<MessageDto>;

public class UploadMessageAttachmentCommandHandler : IRequestHandler<UploadMessageAttachmentCommand, MessageDto>
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public UploadMessageAttachmentCommandHandler(
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<MessageDto> Handle(UploadMessageAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (request.FileBytes is null || request.FileBytes.Length == 0)
            throw new InvalidOperationException("File content is empty.");

        AttachmentUploadValidator.Validate(request.FileName, request.ContentType, request.FileBytes.LongLength);

        var conversation = await _conversationRepository.GetByIdAsync((int)request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation {request.ConversationId} not found.");

        if (conversation.AccountId != (int)request.AccountId)
            throw new InvalidOperationException("Conversation does not belong to this account.");

        var safeName = AttachmentUploadValidator.SanitizeFileName(request.FileName);
        var extension = Path.GetExtension(safeName).TrimStart('.');
        var key = $"attachments/{conversation.AccountId}/{Guid.NewGuid():N}/{safeName}";

        using var stream = new MemoryStream(request.FileBytes);
        await _storageService.UploadFileAsync(key, stream, request.ContentType, cancellationToken);

        var attachment = new Attachment
        {
            AccountId = conversation.AccountId,
            ExternalUrl = key,
            FileName = safeName,
            FileSize = request.FileBytes.LongLength,
            ContentType = request.ContentType,
            FallbackTitle = safeName,
            Extension = extension,
            FileType = AttachmentUploadValidator.ResolveFileType(request.ContentType),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var message = new Message
        {
            ConversationId = conversation.Id,
            AccountId = conversation.AccountId,
            SenderId = request.SenderId,
            SenderType = "agent",
            Content = string.IsNullOrWhiteSpace(request.Caption) ? safeName : request.Caption,
            ContentType = "attachment",
            MessageType = MessageType.Outgoing,
            Status = MessageStatus.Sent,
            Private = request.IsPrivate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Attachments = new List<Attachment> { attachment }
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        conversation.LastActivityAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;
        _conversationRepository.Update(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new MessageCreatedEvent(message.Id, conversation.Id, conversation.AccountId),
            cancellationToken);

        var attachmentDto = new AttachmentDto(
            Id: attachment.Id,
            MessageId: attachment.MessageId,
            FileType: attachment.FileType.ToString().ToLowerInvariant(),
            FileName: attachment.FileName,
            FileUrl: _storageService.GetFileUrl(attachment.ExternalUrl),
            FileSize: attachment.FileSize,
            ContentType: attachment.ContentType,
            ThumbUrl: _storageService.GetFileUrl(attachment.ThumbnailUrl),
            Extension: attachment.Extension,
            CreatedAt: attachment.CreatedAt);

        return new MessageDto(
            message.Id,
            message.ConversationId,
            message.AccountId,
            message.SenderId,
            message.SenderType,
            message.Content,
            message.ContentType,
            message.MessageType.ToString().ToLowerInvariant(),
            message.Private,
            message.Status.ToString().ToLowerInvariant(),
            message.SentAt,
            message.CreatedAt,
            new List<AttachmentDto> { attachmentDto });
    }
}
