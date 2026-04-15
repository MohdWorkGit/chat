using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Application.Hubs;

/// <summary>
/// Real-time hub used by the dashboard and embedded chat widget to receive
/// conversation, message, and typing events.
///
/// This type is both the hub that clients connect to (mapped from the Api
/// layer's Program.cs) AND the type used by <see cref="IHubContext{THub}"/>
/// in the Application layer's event handlers. It MUST be a single type —
/// previously a derived hub was registered in the Api layer while the
/// Application layer resolved <c>IHubContext</c> against this base class,
/// which produced two separate hub lifetime managers and meant broadcasts
/// never reached any connected client.
/// </summary>
public class ConversationHub : Hub
{
    private readonly ILogger<ConversationHub> _logger;

    public ConversationHub(ILogger<ConversationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var accountId = Context.User?.FindFirst("account_id")?.Value
            ?? Context.User?.FindFirst("AccountId")?.Value;

        if (!string.IsNullOrEmpty(accountId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"account_{accountId}");
            _logger.LogInformation("Client {ConnectionId} connected and joined account group {AccountId}",
                Context.ConnectionId, accountId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinAccountGroup(long accountId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"account_{accountId}");
        _logger.LogDebug("Client {ConnectionId} joined account group {AccountId}",
            Context.ConnectionId, accountId);
    }

    public async Task LeaveAccountGroup(long accountId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"account_{accountId}");
        _logger.LogDebug("Client {ConnectionId} left account group {AccountId}",
            Context.ConnectionId, accountId);
    }

    public async Task JoinConversation(long conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogDebug("Client {ConnectionId} joined conversation {ConversationId}",
            Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(long conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogDebug("Client {ConnectionId} left conversation {ConversationId}",
            Context.ConnectionId, conversationId);
    }

    public async Task SendTypingStatus(long conversationId, bool isTyping)
    {
        await Clients.OthersInGroup($"conversation_{conversationId}")
            .SendAsync("TypingStatus", new
            {
                ConversationId = conversationId,
                UserId = Context.UserIdentifier,
                IsTyping = isTyping
            });
    }
}
