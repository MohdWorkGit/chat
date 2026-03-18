using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Auth.Queries;

public record GetCurrentUserQuery(long UserId) : IRequest<UserDto?>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    private readonly IRepository<User> _userRepository;

    public GetCurrentUserQueryHandler(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync((int)request.UserId, cancellationToken);
        if (user is null)
            return null;

        return new UserDto(
            user.Id,
            user.Name,
            user.DisplayName,
            user.Email ?? string.Empty,
            user.AvailabilityStatus.ToString(),
            user.Avatar,
            null,
            user.CreatedAt);
    }
}
