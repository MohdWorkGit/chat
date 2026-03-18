using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Teams.Commands;

public record CreateTeamCommand(
    long AccountId,
    string Name,
    string? Description,
    bool AllowAutoAssign = true) : IRequest<TeamDto>;

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, TeamDto>
{
    private readonly IRepository<Team> _teamRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTeamCommandHandler(IRepository<Team> teamRepository, IUnitOfWork unitOfWork)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<TeamDto> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = new Team
        {
            AccountId = (int)request.AccountId,
            Name = request.Name,
            Description = request.Description,
            AllowAutoAssign = request.AllowAutoAssign,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _teamRepository.AddAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TeamDto(team.Id, team.Name, team.Description, team.AllowAutoAssign, 0);
    }
}
