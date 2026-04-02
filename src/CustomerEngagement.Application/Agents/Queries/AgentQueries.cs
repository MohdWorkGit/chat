using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Agents.Queries;

public record GetAgentsQuery(long AccountId) : IRequest<object>;

public record GetAgentByIdQuery(long AccountId, long Id) : IRequest<object>;

public class GetAgentsQueryHandler : IRequestHandler<GetAgentsQuery, object>
{
    private readonly IRepository<AccountUser> _accountUserRepository;

    public GetAgentsQueryHandler(IRepository<AccountUser> accountUserRepository)
    {
        _accountUserRepository = accountUserRepository ?? throw new ArgumentNullException(nameof(accountUserRepository));
    }

    public async Task<object> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
    {
        var accountUsers = await _accountUserRepository.FindAsync(
            au => au.AccountId == (int)request.AccountId, cancellationToken);

        return accountUsers.Select(au => new
        {
            au.Id,
            au.AccountId,
            au.UserId,
            Role = au.Role.ToString(),
            User = au.User != null ? new
            {
                au.User.Id,
                au.User.Name,
                au.User.Email,
                au.User.AvailabilityStatus,
                au.User.AvatarUrl
            } : null
        }).ToList();
    }
}

public class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, object>
{
    private readonly IRepository<AccountUser> _accountUserRepository;

    public GetAgentByIdQueryHandler(IRepository<AccountUser> accountUserRepository)
    {
        _accountUserRepository = accountUserRepository ?? throw new ArgumentNullException(nameof(accountUserRepository));
    }

    public async Task<object> Handle(GetAgentByIdQuery request, CancellationToken cancellationToken)
    {
        var accountUsers = await _accountUserRepository.FindAsync(
            au => au.AccountId == (int)request.AccountId && au.Id == (int)request.Id,
            cancellationToken);

        var accountUser = accountUsers.FirstOrDefault();

        if (accountUser is null)
            return new { Error = "Agent not found" };

        return new
        {
            accountUser.Id,
            accountUser.AccountId,
            accountUser.UserId,
            Role = accountUser.Role.ToString(),
            User = accountUser.User != null ? new
            {
                accountUser.User.Id,
                accountUser.User.Name,
                accountUser.User.Email,
                accountUser.User.AvailabilityStatus,
                accountUser.User.AvatarUrl
            } : null
        };
    }
}
