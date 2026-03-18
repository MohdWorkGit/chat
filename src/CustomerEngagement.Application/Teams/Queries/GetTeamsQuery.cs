using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Teams.Queries;

public record GetTeamsQuery(long AccountId) : IRequest<IReadOnlyList<TeamDto>>;

public class GetTeamsQueryHandler : IRequestHandler<GetTeamsQuery, IReadOnlyList<TeamDto>>
{
    private readonly IRepository<Team> _teamRepository;

    public GetTeamsQueryHandler(IRepository<Team> teamRepository)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
    }

    public async Task<IReadOnlyList<TeamDto>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var teams = await _teamRepository.ListAsync(
            new { AccountId = (int)request.AccountId }, cancellationToken);

        return teams
            .Select(t => new TeamDto(t.Id, t.Name, t.Description, t.AllowAutoAssign, t.TeamMembers.Count))
            .ToList()
            .AsReadOnly();
    }
}
