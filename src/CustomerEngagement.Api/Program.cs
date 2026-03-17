using System.Text;
using System.Threading.RateLimiting;
using CustomerEngagement.Api.Hubs;
using CustomerEngagement.Api.Middleware;
using CustomerEngagement.Application;
using CustomerEngagement.Domain.Entities;
using CustomerEngagement.Domain.Interfaces;
using CustomerEngagement.Infrastructure.Data;
using CustomerEngagement.Infrastructure.Repositories;
using CustomerEngagement.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        npgsql.EnableRetryOnFailure(3);
    }));

// ---------------------------------------------------------------------------
// ASP.NET Core Identity
// ---------------------------------------------------------------------------
builder.Services.AddIdentity<User, IdentityRole<long>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
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
    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("Agent", policy => policy.RequireRole("Agent", "Administrator"));
});

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

    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    options.AddSlidingWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.SegmentsPerWindow = 2;
        limiter.QueueLimit = 0;
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
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// ---------------------------------------------------------------------------
// Health Checks
// ---------------------------------------------------------------------------
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRedis(redisUrl, name: "redis");

// ---------------------------------------------------------------------------
// Controllers
// ---------------------------------------------------------------------------
builder.Services.AddControllers();

// =========================================================================
// Build & Configure Pipeline
// =========================================================================
var app = builder.Build();

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

app.Run();
