using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Api.Authorization;

/// <summary>
/// Resource-level authorization requirements and handlers implementing
/// the 23+ policies defined in the project specification.
/// </summary>
public static class ResourcePolicies
{
    // Conversation policies
    public const string ConversationRead = "conversation:read";
    public const string ConversationWrite = "conversation:write";
    public const string ConversationDelete = "conversation:delete";

    // Contact policies
    public const string ContactRead = "contact:read";
    public const string ContactWrite = "contact:write";
    public const string ContactDelete = "contact:delete";
    public const string ContactMerge = "contact:merge";

    // Inbox policies
    public const string InboxRead = "inbox:read";
    public const string InboxWrite = "inbox:write";
    public const string InboxDelete = "inbox:delete";

    // Team policies
    public const string TeamRead = "team:read";
    public const string TeamWrite = "team:write";
    public const string TeamDelete = "team:delete";

    // Label policies
    public const string LabelRead = "label:read";
    public const string LabelWrite = "label:write";
    public const string LabelDelete = "label:delete";

    // Report policies
    public const string ReportRead = "report:read";
    public const string ReportWrite = "report:write";

    // Webhook policies
    public const string WebhookRead = "webhook:read";
    public const string WebhookWrite = "webhook:write";
    public const string WebhookDelete = "webhook:delete";

    // Automation policies
    public const string AutomationRead = "automation:read";
    public const string AutomationWrite = "automation:write";
    public const string AutomationDelete = "automation:delete";

    // Knowledge Base policies
    public const string KnowledgeBaseRead = "knowledge_base:read";
    public const string KnowledgeBaseWrite = "knowledge_base:write";

    // Account management
    public const string AccountWrite = "account:write";
    public const string UserManage = "user:manage";

    // Broadcast/Campaign policies
    public const string BroadcastRead = "broadcast:read";
    public const string BroadcastWrite = "broadcast:write";

    /// <summary>
    /// Returns all policy names for registration.
    /// </summary>
    public static IReadOnlyList<string> All =>
    [
        ConversationRead, ConversationWrite, ConversationDelete,
        ContactRead, ContactWrite, ContactDelete, ContactMerge,
        InboxRead, InboxWrite, InboxDelete,
        TeamRead, TeamWrite, TeamDelete,
        LabelRead, LabelWrite, LabelDelete,
        ReportRead, ReportWrite,
        WebhookRead, WebhookWrite, WebhookDelete,
        AutomationRead, AutomationWrite, AutomationDelete,
        KnowledgeBaseRead, KnowledgeBaseWrite,
        AccountWrite, UserManage,
        BroadcastRead, BroadcastWrite
    ];

    /// <summary>
    /// Policies that require Administrator role. All others are accessible by both Agent and Administrator.
    /// </summary>
    public static IReadOnlySet<string> AdminOnly { get; } = new HashSet<string>
    {
        InboxWrite, InboxDelete,
        TeamWrite, TeamDelete,
        WebhookWrite, WebhookDelete,
        AutomationWrite, AutomationDelete,
        AccountWrite, UserManage,
        BroadcastWrite,
        ContactDelete, ContactMerge,
        ReportWrite,
        LabelDelete,
        KnowledgeBaseWrite,
    };
}

/// <summary>
/// Requirement for resource-level authorization.
/// </summary>
public class ResourcePermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public ResourcePermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Handles resource-level authorization by checking the user's role,
/// custom role permissions, and account membership.
/// </summary>
public class ResourceAuthorizationHandler : AuthorizationHandler<ResourcePermissionRequirement>
{
    private readonly ILogger<ResourceAuthorizationHandler> _logger;

    public ResourceAuthorizationHandler(ILogger<ResourceAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourcePermissionRequirement requirement)
    {
        var user = context.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            return Task.CompletedTask;
        }

        // Super admins bypass all resource checks
        if (user.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var accountRole = user.FindFirst("account_role")?.Value
            ?? user.FindFirst("AccountRole")?.Value;

        if (string.IsNullOrEmpty(accountRole))
        {
            _logger.LogWarning("User has no account_role claim for permission check: {Permission}", requirement.Permission);
            return Task.CompletedTask;
        }

        // Administrators have all permissions
        if (string.Equals(accountRole, "Administrator", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Agents can access non-admin-only policies
        if (string.Equals(accountRole, "Agent", StringComparison.OrdinalIgnoreCase))
        {
            if (!ResourcePolicies.AdminOnly.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check custom role permissions claim for admin-only policies
            var customPermissions = user.FindFirst("custom_permissions")?.Value;
            if (!string.IsNullOrEmpty(customPermissions))
            {
                var permissions = customPermissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (permissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            _logger.LogWarning("Agent denied permission: {Permission}", requirement.Permission);
        }

        return Task.CompletedTask;
    }
}
