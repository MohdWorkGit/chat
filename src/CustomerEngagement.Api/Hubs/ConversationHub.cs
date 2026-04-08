using Microsoft.AspNetCore.SignalR;
using AppHub = CustomerEngagement.Application.Hubs;

namespace CustomerEngagement.Api.Hubs;

// NOTE: This hub is intentionally not marked [Authorize] so the embeddable
// widget (which has no user JWT, only a website token) can also subscribe
// to conversation events. Dashboard users still authenticate implicitly —
// the JwtBearerHandler attaches the principal to Context.User whenever a
// valid bearer/access_token is provided, so account-scoped grouping in
// OnConnectedAsync keeps working for logged-in agents.
public class ConversationHub : AppHub.ConversationHub
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
