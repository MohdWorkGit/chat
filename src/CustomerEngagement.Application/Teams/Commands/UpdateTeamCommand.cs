using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Teams.Commands;

public record UpdateTeamCommand(
    long AccountId,
    long Id,
    string? Name,
    string? Description,
    bool? AllowAutoAssign) : IRequest<TeamDto>;

public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, TeamDto>
{
    private readonly IRepository<Team> _teamRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTeamCommandHandler(IRepository<Team> teamRepository, IUnitOfWork unitOfWork)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<TeamDto> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetByIdAsync((int)request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Team {request.Id} not found.");

        if (request.Name is not null) team.Name = request.Name;
        if (request.Description is not null) team.Description = request.Description;
        if (request.AllowAutoAssign.HasValue) team.AllowAutoAssign = request.AllowAutoAssign.Value;
        team.UpdatedAt = DateTime.UtcNow;

        await _teamRepository.UpdateAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TeamDto(team.Id, team.Name, team.Description, team.AllowAutoAssign, team.TeamMembers.Count);
    }
}
