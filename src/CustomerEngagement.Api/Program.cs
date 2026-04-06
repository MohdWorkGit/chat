using System.Text;
using System.Threading.RateLimiting;
using CustomerEngagement.Api.Authorization;
using CustomerEngagement.Api.Hubs;
using CustomerEngagement.Api.Middleware;
using CustomerEngagement.Application;
using CustomerEngagement.Application.Auth;
using CustomerEngagement.Application.Services.Automations;
using CustomerEngagement.Application.Services.Channels;
using CustomerEngagement.Application.Services.Contacts;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Application.Services.HelpCenter;
using CustomerEngagement.Application.Services.Integrations;
using CustomerEngagement.Application.Services.Notifications;
using CustomerEngagement.Application.Services.Reporting;
using CustomerEngagement.Application.Services.Search;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using CustomerEngagement.Enterprise.Captain.Services;
using CustomerEngagement.Enterprise.AuditLogs.Services;
using CustomerEngagement.Enterprise.CustomRoles.Services;
using CustomerEngagement.Enterprise.Saml.Services;
using CustomerEngagement.Infrastructure.ExternalServices.Email;
using CustomerEngagement.Infrastructure.ExternalServices.GeoIp;
using CustomerEngagement.Infrastructure.ExternalServices.Push;
using CustomerEngagement.Infrastructure.ExternalServices.Storage;
using CustomerEngagement.Infrastructure.Identity;
using CustomerEngagement.Infrastructure.Persistence;
using CustomerEngagement.Infrastructure.Repositories;
using CustomerEngagement.Application.BackgroundJobs;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Serilog
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// ---------------------------------------------------------------------------
// Database – EF Core + PostgreSQL
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        npgsql.EnableRetryOnFailure(3);
        npgsql.UseVector();
    }));

// ---------------------------------------------------------------------------
// ASP.NET Core Identity
// ---------------------------------------------------------------------------
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ---------------------------------------------------------------------------
// JWT Authentication
// ---------------------------------------------------------------------------
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT secret is not configured.");

var jwtExpiryMinutes = int.Parse(
    builder.Configuration["JWT_EXPIRY_MINUTES"]
    ?? builder.Configuration["Jwt:ExpiryMinutes"]
    ?? "60");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CustomerEngagement",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CustomerEngagement",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        // Allow SignalR to receive the JWT via query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// ---------------------------------------------------------------------------
// Authorization Policies
// ---------------------------------------------------------------------------
builder.Services.AddAuthorization(options =>
{
    // Basic role-based policies
    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("Agent", policy => policy.RequireRole("Agent", "Administrator"));

    // Resource-level authorization policies (23+ policies per spec)
    foreach (var policyName in ResourcePolicies.All)
    {
        options.AddPolicy(policyName, policy =>
            policy.Requirements.Add(new ResourcePermissionRequirement(policyName)));
    }
});

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, ResourceAuthorizationHandler>();

// ---------------------------------------------------------------------------
// SignalR
// ---------------------------------------------------------------------------
var redisUrl = builder.Configuration["REDIS_URL"]
    ?? builder.Configuration["Redis:Url"]
    ?? "localhost:6379";

var signalRBuilder = builder.Services.AddSignalR();
if (!builder.Environment.IsDevelopment())
{
    signalRBuilder.AddStackExchangeRedis(redisUrl);
}

// ---------------------------------------------------------------------------
// Redis Distributed Cache
// ---------------------------------------------------------------------------
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisUrl;
    options.InstanceName = "CustomerEngagement:";
});

// ---------------------------------------------------------------------------
// Hangfire
// ---------------------------------------------------------------------------
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

// ---------------------------------------------------------------------------
// CORS
// ---------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---------------------------------------------------------------------------
// Rate Limiting
// ---------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // General API rate limit: 100 requests/minute per IP
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    // Auth endpoints (login/signup): 10 requests/minute per IP
    options.AddSlidingWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.SegmentsPerWindow = 2;
        limiter.QueueLimit = 0;
    });

    // Widget endpoints: 300 requests/minute (higher limit for embedded widgets)
    options.AddFixedWindowLimiter("widget", limiter =>
    {
        limiter.PermitLimit = 300;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    // Search endpoints: 30 requests/minute per IP
    options.AddSlidingWindowLimiter("search", limiter =>
    {
        limiter.PermitLimit = 30;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.SegmentsPerWindow = 3;
        limiter.QueueLimit = 0;
    });

    // Report generation: 10 requests/minute (expensive queries)
    options.AddFixedWindowLimiter("reports", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    // Per-account global rate limit: 1000 requests/minute
    options.AddFixedWindowLimiter("account", limiter =>
    {
        limiter.PermitLimit = 1000;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    // Global fallback partitioner using remote IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 500,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

// ---------------------------------------------------------------------------
// Swagger / OpenAPI
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Customer Engagement API",
        Version = "v1",
        Description = "API for the Customer Engagement Platform"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ---------------------------------------------------------------------------
// MediatR
// ---------------------------------------------------------------------------
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>());

// ---------------------------------------------------------------------------
// Infrastructure Services
// ---------------------------------------------------------------------------
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IContactMergeService, ContactMergeService>();
builder.Services.AddScoped<IContactImportService, ContactImportService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IPortalService, PortalService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<JwtTokenService>();

// Automation & workflow services
builder.Services.AddScoped<IAutomationRuleEngine, AutomationRuleEngine>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IMacroExecutionService, MacroExecutionService>();

// Channel services
builder.Services.AddScoped<IEmailChannelService, EmailChannelService>();
builder.Services.AddScoped<IWebWidgetService, WebWidgetService>();
builder.Services.AddScoped<IApiChannelService, ApiChannelService>();

// Notification services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

// Infrastructure email & push services
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IEmailReceiver, ImapEmailReceiver>();
builder.Services.AddSingleton<IWebPushSender, VapidWebPushService>();

// Integration services
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHttpClient<IRasaNluService, RasaNluService>();

// Reporting services
builder.Services.AddScoped<IReportBuilder, ReportBuilder>();
builder.Services.AddScoped<ICsatReportService, CsatReportService>();

// Storage & GeoIP services
builder.Services.AddScoped<IStorageService, MinioStorageService>();
builder.Services.AddSingleton<IGeoIpService, MaxMindOfflineGeoIpService>();

// Enterprise services
builder.Services.AddScoped<ISamlAuthService, SamlAuthService>();
builder.Services.AddScoped<ICustomRoleService, CustomRoleService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Enterprise – Captain AI services
// Register DbContext so Enterprise services can resolve it from AppDbContext
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddHttpClient("WebhookDelivery");
builder.Services.AddHttpClient("AvatarFetch");
// Typed clients that also bind the interface, so resolving the interface goes
// through the HttpClientFactory-backed constructor.
builder.Services.AddHttpClient<IAssistantChatService, AssistantChatService>();
builder.Services.AddHttpClient<ICopilotService, CopilotService>();
builder.Services.AddHttpClient<IEmbeddingService, EmbeddingService>();
builder.Services.AddHttpClient<IToolRegistryService, ToolRegistryService>();

// Search & other services
builder.Services.AddScoped<IGlobalSearchService, GlobalSearchService>();
builder.Services.AddScoped<IFilterService, FilterService>();
builder.Services.AddScoped<ITypingStatusManager, TypingStatusManager>();
builder.Services.AddScoped<IContactSearchService, ContactSearchService>();

// ---------------------------------------------------------------------------
// Health Checks
// ---------------------------------------------------------------------------
var ollamaUrl = builder.Configuration["OLLAMA_URL"] ?? builder.Configuration["Ollama:Url"] ?? "http://localhost:11434";
var rasaUrl = builder.Configuration["RASA_URL"] ?? builder.Configuration["Rasa:Url"] ?? "http://localhost:5005";
var minioEndpoint = builder.Configuration["MINIO_ENDPOINT"] ?? builder.Configuration["Minio:Endpoint"] ?? "localhost:9000";

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRedis(redisUrl, name: "redis")
    .AddUrlGroup(new Uri($"{ollamaUrl}/api/tags"), name: "ollama", tags: new[] { "ai" })
    .AddUrlGroup(new Uri($"{rasaUrl}/status"), name: "rasa", tags: new[] { "ai" })
    .AddUrlGroup(new Uri($"http://{minioEndpoint}/minio/health/live"), name: "minio", tags: new[] { "storage" });

// ---------------------------------------------------------------------------
// Controllers
// ---------------------------------------------------------------------------
builder.Services.AddControllers();

// =========================================================================
// Build & Configure Pipeline
// =========================================================================
var app = builder.Build();

// Auto-apply EF Core migrations on startup
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Migration failed — falling back to EnsureCreated for initial setup");

        // EnsureCreated only creates schema when it also creates the database.
        // If the database already exists (from a previous failed attempt), it returns
        // false and does nothing — even if no tables exist.
        var created = db.Database.EnsureCreated();
        if (!created)
        {
            // Database exists — check if tables are actually present using the raw connection.
            var conn = db.Database.GetDbConnection();
            var wasOpen = conn.State == System.Data.ConnectionState.Open;
            if (!wasOpen) await conn.OpenAsync();
            bool hasSchema;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetRoles')";
                hasSchema = (bool)(await cmd.ExecuteScalarAsync())!;
            }
            if (!wasOpen) await conn.CloseAsync();

            if (!hasSchema)
            {
                Log.Warning("Database exists but schema is missing — recreating");
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
        }
        Log.Information("Database schema ensured via EnsureCreated");
    }

    // Seed roles, default account, and admin user
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    string[] roles = ["Administrator", "Agent"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
            Log.Information("Created role: {Role}", role);
        }
    }

    // Ensure default account exists
    if (!db.Set<Account>().Any())
    {
        var account = new Account { Name = "Default Account", Locale = "en", AutoResolveAfterDays = 14 };
        db.Set<Account>().Add(account);
        await db.SaveChangesAsync();
        Log.Information("Created default account (Id={AccountId})", account.Id);
    }

    // Seed admin user
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    const string adminEmail = "admin@example.com";
    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            Name = "Admin",
            Provider = "email",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
            var defaultAccount = db.Set<Account>().First();
            db.Set<AccountUser>().Add(new AccountUser
            {
                AccountId = defaultAccount.Id,
                UserId = adminUser.Id,
                Role = CustomerEngagement.Core.Enums.UserRole.Administrator
            });
            await db.SaveChangesAsync();
            Log.Information("Created admin user: {Email} (password: Admin123!)", adminEmail);
        }
        else
        {
            Log.Error("Failed to create admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Engagement API v1"));
}

app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TenantMiddleware>();

// Endpoints
app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversation");
app.MapHangfireDashboard("/hangfire");
app.MapHealthChecks("/health");

// ---------------------------------------------------------------------------
// Hangfire Recurring Jobs
// ---------------------------------------------------------------------------
var jobManager = app.Services.GetRequiredService<IRecurringJobManager>();

// Conversation lifecycle jobs
jobManager.AddOrUpdate<ConversationAutoResolveJob>(
    "auto-resolve-conversations", job => job.ExecuteAsync(CancellationToken.None), Cron.Hourly);

jobManager.AddOrUpdate<ReopenSnoozedConversationsJob>(
    "reopen-snoozed-conversations", job => job.ExecuteAsync(CancellationToken.None), "*/5 * * * *");

// Email & channel jobs
jobManager.AddOrUpdate<ImapEmailFetchJob>(
    "imap-email-fetch", job => job.ExecuteAsync(CancellationToken.None), "*/2 * * * *");

// Campaign jobs
jobManager.AddOrUpdate<CampaignTriggerJob>(
    "campaign-trigger", job => job.ExecuteAsync(CancellationToken.None), "*/10 * * * *");

jobManager.AddOrUpdate<ScheduledCampaignJob>(
    "ongoing-campaign-evaluation", job => job.ExecuteAsync(CancellationToken.None), "*/15 * * * *");

// Reporting & analytics
jobManager.AddOrUpdate<ReportGenerationJob>(
    "daily-report-generation", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily(2));

// Template sync
jobManager.AddOrUpdate<TemplateSyncJob>(
    "template-sync", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily(3));

// Data cleanup
jobManager.AddOrUpdate<CleanupJob>(
    "data-cleanup", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily(4));

app.Run();
