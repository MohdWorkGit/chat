using CustomerEngagement.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CustomerEngagement.Application.Profiles.Queries;

public record GetProfileQuery(long UserId) : IRequest<object>;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, object>
{
    private readonly UserManager<User> _userManager;

    public GetProfileQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<object> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
            return new { Error = "User not found" };

        return new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Avatar,
            user.AvatarUrl,
            AvailabilityStatus = user.AvailabilityStatus.ToString(),
            user.DisplayName,
            user.MessageSignature
        };
    }
}
