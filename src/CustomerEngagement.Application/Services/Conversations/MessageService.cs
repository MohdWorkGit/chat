using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Services.Conversations;

public class MessageService : IMessageService
{
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public MessageService(
        IRepository<Message> messageRepository,
        IRepository<Attachment> attachmentRepository,
        IRepository<Conversation> conversationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<PaginatedResultDto<MessageDto>> GetByConversationAsync(
        long conversationId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var messages = await _messageRepository.ListAsync(
            new { ConversationId = (int)conversationId },
            cancellationToken);

        var totalCount = messages.Count;

        var items = messages
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PaginatedResultDto<MessageDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<MessageDto> CreateAsync(
        long conversationId,
        CreateMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync((int)conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        var message = new Message
        {
            ConversationId = (int)conversationId,
            AccountId = conversation.AccountId,
            Content = request.Content,
            MessageType = (MessageType)request.MessageType,
            SenderId = request.SenderId,
            SenderType = request.SenderType,
            Private = request.IsPrivate,
            ContentType = request.ContentType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Handle attachments
        if (request.Attachments is { Count: > 0 })
        {
            foreach (var attachmentRequest in request.Attachments)
            {
                var attachment = new Attachment
                {
                    MessageId = message.Id,
                    AccountId = conversation.AccountId,
                    FallbackTitle = attachmentRequest.FileName,
                    ExternalUrl = attachmentRequest.FileUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _attachmentRepository.AddAsync(attachment, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Update conversation timestamp
        conversation.UpdatedAt = DateTime.UtcNow;
        conversation.LastActivityAt = DateTime.UtcNow;
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new MessageCreatedEvent(message.Id, (int)conversationId, conversation.AccountId),
            cancellationToken);

        return MapToDto(message);
    }

    public async Task<MessageDto> UpdateAsync(
        long messageId,
        UpdateMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync((int)messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Message {messageId} not found.");

        message.Content = request.Content;
        message.UpdatedAt = DateTime.UtcNow;

        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(message);
    }

    public async Task DeleteAsync(long messageId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync((int)messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Message {messageId} not found.");

        await _messageRepository.DeleteAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static MessageDto MapToDto(Message message)
    {
        var attachments = message.Attachments
            .Select(a => new AttachmentDto(a.Id, a.FileType.ToString(), a.ExternalUrl, a.Extension, a.FallbackTitle))
            .ToList();
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
            attachments);
    }
}
