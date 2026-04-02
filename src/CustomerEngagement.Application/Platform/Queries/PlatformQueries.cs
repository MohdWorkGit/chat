using System.Linq.Expressions;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Platform.Queries;

public record GetPlatformAccountsQuery(int Page, int PageSize) : IRequest<object>;

public record GetPlatformAccountByIdQuery(long Id) : IRequest<object>;

public record GetAgentBotsQuery(int Page, int PageSize) : IRequest<object>;

public record GetAgentBotByIdQuery(long Id) : IRequest<object>;

public record GetPlatformUsersQuery(int Page, int PageSize) : IRequest<object>;

public record GetPlatformUserByIdQuery(long Id) : IRequest<object>;

public class GetPlatformAccountsQueryHandler : IRequestHandler<GetPlatformAccountsQuery, object>
{
    private readonly IRepository<Account> _repository;

    public GetPlatformAccountsQueryHandler(IRepository<Account> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPlatformAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _repository.GetPagedAsync(
            request.Page, request.PageSize, null, a => a.CreatedAt, ascending: false, cancellationToken);

        var totalCount = await _repository.CountAsync(null, cancellationToken);

        return new
        {
            Data = accounts.Select(a => new
            {
                a.Id,
                a.Name,
                a.Locale,
                a.Domain,
                a.AutoResolveAfterDays,
                a.SupportEmail,
                a.CreatedAt,
                a.UpdatedAt
            }),
            Meta = new
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        };
    }
}

public class GetPlatformAccountByIdQueryHandler : IRequestHandler<GetPlatformAccountByIdQuery, object>
{
    private readonly IRepository<Account> _repository;

    public GetPlatformAccountByIdQueryHandler(IRepository<Account> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPlatformAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (account is null)
            return null!;

        return new
        {
            account.Id,
            account.Name,
            account.Locale,
            account.Domain,
            account.AutoResolveAfterDays,
            account.SupportEmail,
            account.CreatedAt,
            account.UpdatedAt
        };
    }
}

public class GetAgentBotsQueryHandler : IRequestHandler<GetAgentBotsQuery, object>
{
    private readonly IRepository<AgentBot> _repository;

    public GetAgentBotsQueryHandler(IRepository<AgentBot> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAgentBotsQuery request, CancellationToken cancellationToken)
    {
        var bots = await _repository.GetPagedAsync(
            request.Page, request.PageSize, null, b => b.CreatedAt, ascending: false, cancellationToken);

        var totalCount = await _repository.CountAsync(null, cancellationToken);

        return new
        {
            Data = bots.Select(b => new
            {
                b.Id,
                b.Name,
                b.Description,
                b.OutgoingUrl,
                b.BotType,
                b.AccountId,
                b.CreatedAt,
                b.UpdatedAt
            }),
            Meta = new
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        };
    }
}

public class GetAgentBotByIdQueryHandler : IRequestHandler<GetAgentBotByIdQuery, object>
{
    private readonly IRepository<AgentBot> _repository;

    public GetAgentBotByIdQueryHandler(IRepository<AgentBot> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAgentBotByIdQuery request, CancellationToken cancellationToken)
    {
        var bot = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (bot is null)
            return null!;

        return new
        {
            bot.Id,
            bot.Name,
            bot.Description,
            bot.OutgoingUrl,
            bot.BotType,
            bot.AccountId,
            bot.CreatedAt,
            bot.UpdatedAt
        };
    }
}

public class GetPlatformUsersQueryHandler : IRequestHandler<GetPlatformUsersQuery, object>
{
    private readonly IRepository<User> _repository;

    public GetPlatformUsersQueryHandler(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPlatformUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _repository.GetPagedAsync(
            request.Page, request.PageSize, null, u => u.CreatedAt, ascending: false, cancellationToken);

        var totalCount = await _repository.CountAsync(null, cancellationToken);

        return new
        {
            Data = users.Select(u => new
            {
                u.Id,
                u.Name,
                u.DisplayName,
                u.Email,
                u.AvailabilityStatus,
                u.AvatarUrl,
                u.ConfirmedAt,
                u.CreatedAt,
                u.UpdatedAt
            }),
            Meta = new
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        };
    }
}

public class GetPlatformUserByIdQueryHandler : IRequestHandler<GetPlatformUserByIdQuery, object>
{
    private readonly IRepository<User> _repository;

    public GetPlatformUserByIdQueryHandler(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetPlatformUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (user is null)
            return null!;

        return new
        {
            user.Id,
            user.Name,
            user.DisplayName,
            user.Email,
            user.AvailabilityStatus,
            user.AvatarUrl,
            user.ConfirmedAt,
            user.CreatedAt,
            user.UpdatedAt
        };
    }
}
