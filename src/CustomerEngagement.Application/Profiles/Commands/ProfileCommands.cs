using MediatR;
using Microsoft.AspNetCore.Http;

namespace CustomerEngagement.Application.Profiles.Commands;

public record UpdateProfileCommand(long UserId = 0, string? DisplayName = null, string? Email = null) : IRequest<object>;

public record UpdateAvailabilityCommand(long UserId = 0, string? Availability = null) : IRequest<object>;

public record UpdateAvatarCommand(long UserId, IFormFile File) : IRequest<object>;

public record DeleteAvatarCommand(long UserId) : IRequest<object>;
