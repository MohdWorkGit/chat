using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Events;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.BackgroundJobs;

public class ConversationAutoResolveJob
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<ConversationAutoResolveJob> _logger;

    public ConversationAutoResolveJob(
        IRepository<Account> accountRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<ConversationAutoResolveJob> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Automatically resolves open conversations with no recent activity.
    /// Intended to be scheduled by Hangfire as a daily recurring job.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting conversation auto-resolve job");

        var accounts = await _accountRepository.GetAllAsync(cancellationToken);
        var totalResolved = 0;

        foreach (var account in accounts)
        {
            try
            {
                var resolved = await AutoResolveForAccountAsync(account, cancellationToken);
                totalResolved += resolved;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-resolve conversations for account {AccountId}", account.Id);
            }
        }

        _logger.LogInformation("Conversation auto-resolve job completed. Total resolved: {Count}", totalResolved);
    }

    private async Task<int> AutoResolveForAccountAsync(Account account, CancellationToken cancellationToken)
    {
        if (account.AutoResolveAfterDays <= 0)
            return 0;

        var cutoffDate = DateTime.UtcNow.AddDays(-account.AutoResolveAfterDays);

        var staleConversations = await _conversationRepository.FindAsync(
            c => c.AccountId == account.Id
                 && c.Status == ConversationStatus.Open
                 && c.UpdatedAt < cutoffDate,
            cancellationToken);

        var resolvedCount = 0;

        foreach (var conversation in staleConversations)
        {
            conversation.Status = ConversationStatus.Resolved;
            conversation.UpdatedAt = DateTime.UtcNow;
            await _conversationRepository.UpdateAsync(conversation, cancellationToken);

            // Create activity message
            var activityMessage = new Message
            {
                ConversationId = conversation.Id,
                AccountId = conversation.AccountId,
                Content = "Conversation auto-resolved due to inactivity",
                ContentType = "text/plain",
                MessageType = MessageType.Activity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _messageRepository.AddAsync(activityMessage, cancellationToken);

            await _mediator.Publish(
                new ConversationStatusChangedEvent(
                    conversation.Id,
                    conversation.AccountId,
                    nameof(ConversationStatus.Open),
                    nameof(ConversationStatus.Resolved)),
                cancellationToken);

            resolvedCount++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (resolvedCount > 0)
        {
            _logger.LogInformation(
                "Auto-resolved {Count} conversations for account {AccountId}",
                resolvedCount, account.Id);
        }

        return resolvedCount;
    }
}
