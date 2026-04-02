using System.Linq.Expressions;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.SuperAdmin.Queries;

public record GetAdminAccountsQuery(int Page, int PageSize) : IRequest<object>;

public record GetAdminAccountByIdQuery(long Id) : IRequest<object>;

public record GetAdminUsersQuery(int Page, int PageSize) : IRequest<object>;

public record GetAdminUserByIdQuery(long Id) : IRequest<object>;

public record GetAdminConfigQuery() : IRequest<object>;

public class GetAdminAccountsQueryHandler : IRequestHandler<GetAdminAccountsQuery, object>
{
    private readonly IRepository<Account> _repository;

    public GetAdminAccountsQueryHandler(IRepository<Account> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAdminAccountsQuery request, CancellationToken cancellationToken)
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

public class GetAdminAccountByIdQueryHandler : IRequestHandler<GetAdminAccountByIdQuery, object>
{
    private readonly IRepository<Account> _repository;

    public GetAdminAccountByIdQueryHandler(IRepository<Account> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAdminAccountByIdQuery request, CancellationToken cancellationToken)
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
            account.FeatureFlags,
            account.SettingsFlags,
            account.CreatedAt,
            account.UpdatedAt
        };
    }
}

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, object>
{
    private readonly IRepository<User> _repository;

    public GetAdminUsersQueryHandler(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
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

public class GetAdminUserByIdQueryHandler : IRequestHandler<GetAdminUserByIdQuery, object>
{
    private readonly IRepository<User> _repository;

    public GetAdminUserByIdQueryHandler(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAdminUserByIdQuery request, CancellationToken cancellationToken)
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

public class GetAdminConfigQueryHandler : IRequestHandler<GetAdminConfigQuery, object>
{
    private readonly IRepository<InstallationConfig> _repository;

    public GetAdminConfigQueryHandler(IRepository<InstallationConfig> repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetAdminConfigQuery request, CancellationToken cancellationToken)
    {
        var configs = await _repository.GetAllAsync(cancellationToken);

        return new
        {
            Data = configs.Select(c => new
            {
                c.Id,
                c.Name,
                c.Value,
                c.Locked,
                c.CreatedAt,
                c.UpdatedAt
            })
        };
    }
}
