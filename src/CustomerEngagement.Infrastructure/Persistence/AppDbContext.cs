using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Entities.Channels;
using CustomerEngagement.Enterprise.Captain.Entities;
using CustomerEngagement.Enterprise.CustomRoles.Entities;
using CustomerEngagement.Enterprise.Saml.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomerEngagement.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    private readonly int? _currentAccountId;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _currentAccountId = tenantProvider?.AccountId;
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactInbox> ContactInboxes => Set<ContactInbox>();
    public DbSet<Inbox> Inboxes => Set<Inbox>();
    public DbSet<InboxMember> InboxMembers => Set<InboxMember>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Mention> Mentions => Set<Mention>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<AutomationRule> AutomationRules => Set<AutomationRule>();
    public DbSet<Macro> Macros => Set<Macro>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CannedResponse> CannedResponses => Set<CannedResponse>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<CustomAttributeDefinition> CustomAttributeDefinitions => Set<CustomAttributeDefinition>();
    public DbSet<CustomFilter> CustomFilters => Set<CustomFilter>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationSetting> NotificationSettings => Set<NotificationSetting>();
    public DbSet<NotificationSubscription> NotificationSubscriptions => Set<NotificationSubscription>();
    public DbSet<CsatSurveyResponse> CsatSurveyResponses => Set<CsatSurveyResponse>();
    public DbSet<ReportingEvent> ReportingEvents => Set<ReportingEvent>();
    public DbSet<WorkingHour> WorkingHours => Set<WorkingHour>();
    public DbSet<Portal> Portals => Set<Portal>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<AgentBot> AgentBots => Set<AgentBot>();
    public DbSet<AgentBotInbox> AgentBotInboxes => Set<AgentBotInbox>();
    public DbSet<AccessToken> AccessTokens => Set<AccessToken>();
    public DbSet<DataImport> DataImports => Set<DataImport>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<InstallationConfig> InstallationConfigs => Set<InstallationConfig>();
    public DbSet<PlatformApp> PlatformApps => Set<PlatformApp>();
    public DbSet<PlatformAppPermissible> PlatformAppPermissibles => Set<PlatformAppPermissible>();
    public DbSet<IntegrationHook> IntegrationHooks => Set<IntegrationHook>();
    public DbSet<ChannelWebWidget> ChannelWebWidgets => Set<ChannelWebWidget>();
    public DbSet<ChannelEmail> ChannelEmails => Set<ChannelEmail>();
    public DbSet<ChannelApi> ChannelApis => Set<ChannelApi>();
    public DbSet<AccountUser> AccountUsers => Set<AccountUser>();
    public DbSet<ConversationDraft> ConversationDrafts => Set<ConversationDraft>();
    public DbSet<AssignmentPolicy> AssignmentPolicies => Set<AssignmentPolicy>();
    public DbSet<RelatedCategory> RelatedCategories => Set<RelatedCategory>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<SamlConfig> SamlConfigs => Set<SamlConfig>();
    public DbSet<SamlRoleMapping> SamlRoleMappings => Set<SamlRoleMapping>();

    // Enterprise – Captain AI module
    public DbSet<CaptainAssistant> CaptainAssistants => Set<CaptainAssistant>();
    public DbSet<CaptainDocument> CaptainDocuments => Set<CaptainDocument>();
    public DbSet<CaptainCustomTool> CaptainCustomTools => Set<CaptainCustomTool>();
    public DbSet<CaptainScenario> CaptainScenarios => Set<CaptainScenario>();
    public DbSet<CaptainInbox> CaptainInboxes => Set<CaptainInbox>();
    public DbSet<ArticleEmbedding> ArticleEmbeddings => Set<ArticleEmbedding>();
    public DbSet<CopilotThread> CopilotThreads => Set<CopilotThread>();
    public DbSet<CopilotMessage> CopilotMessages => Set<CopilotMessage>();

    // Enterprise – Custom Roles module
    public DbSet<CustomRole> CustomRoles => Set<CustomRole>();
    public DbSet<CustomRoleAssignment> CustomRoleAssignments => Set<CustomRoleAssignment>();

    // Enterprise – Audit Logs module
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresExtension("vector");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global tenant query filters for multi-tenancy
        if (_currentAccountId.HasValue)
        {
            builder.Entity<Conversation>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Message>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Contact>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Inbox>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<InboxMember>().HasQueryFilter(e => e.InboxId != 0); // filtered via inbox
            builder.Entity<Team>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Label>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Campaign>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<CannedResponse>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Webhook>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<CustomAttributeDefinition>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<CustomFilter>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Notification>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<AutomationRule>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Macro>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Portal>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Article>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Category>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<ReportingEvent>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<WorkingHour>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Note>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<Mention>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<AuditLog>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<ConversationDraft>().HasQueryFilter(e => e.AccountId == _currentAccountId);
            builder.Entity<EmailTemplate>().HasQueryFilter(e => e.AccountId == _currentAccountId);
        }
    }
}

public interface ITenantProvider
{
    int? AccountId { get; }
}
