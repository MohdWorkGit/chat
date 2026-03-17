using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
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
            new { ConversationId = conversationId },
            cancellationToken);

        var totalCount = await _messageRepository.CountAsync(
            new { ConversationId = conversationId },
            cancellationToken);

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
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        var message = new Message
        {
            ConversationId = conversationId,
            AccountId = conversation.AccountId,
            InboxId = conversation.InboxId,
            Content = request.Content,
            MessageType = request.MessageType,
            SenderId = request.SenderId,
            SenderType = request.SenderType,
            IsPrivate = request.IsPrivate,
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
                    FileName = attachmentRequest.FileName,
                    ContentType = attachmentRequest.ContentType,
                    FileSize = attachmentRequest.FileSize,
                    FileUrl = attachmentRequest.FileUrl,
                    ThumbnailUrl = attachmentRequest.ThumbnailUrl,
                    CreatedAt = DateTime.UtcNow
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
            new MessageCreatedEvent(message.Id, conversationId, conversation.AccountId),
            cancellationToken);

        return MapToDto(message);
    }

    public async Task<MessageDto> UpdateAsync(
        long messageId,
        UpdateMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Message {messageId} not found.");

        message.Content = request.Content;
        message.UpdatedAt = DateTime.UtcNow;

        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(message);
    }

    public async Task DeleteAsync(long messageId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Message {messageId} not found.");

        await _messageRepository.DeleteAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static MessageDto MapToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            AccountId = message.AccountId,
            InboxId = message.InboxId,
            Content = message.Content,
            MessageType = message.MessageType,
            SenderId = message.SenderId,
            SenderType = message.SenderType,
            IsPrivate = message.IsPrivate,
            ContentType = message.ContentType,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };
    }
}

public record MessageCreatedEvent(long MessageId, long ConversationId, int AccountId) : INotification;
