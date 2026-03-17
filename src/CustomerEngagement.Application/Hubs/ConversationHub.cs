using Microsoft.AspNetCore.SignalR;

namespace CustomerEngagement.Application.Hubs;

/// <summary>
/// Marker hub class used for IHubContext&lt;ConversationHub&gt; in the Application layer.
/// The Api layer's ConversationHub inherits from this to share the same hub context.
/// </summary>
public class ConversationHub : Hub
{
}
