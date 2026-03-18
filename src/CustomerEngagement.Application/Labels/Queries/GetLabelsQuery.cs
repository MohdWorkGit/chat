using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Labels.Queries;

public record GetLabelsQuery(long AccountId) : IRequest<IReadOnlyList<LabelDto>>;

public class GetLabelsQueryHandler : IRequestHandler<GetLabelsQuery, IReadOnlyList<LabelDto>>
{
    private readonly IRepository<Label> _labelRepository;

    public GetLabelsQueryHandler(IRepository<Label> labelRepository)
    {
        _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
    }

    public async Task<IReadOnlyList<LabelDto>> Handle(GetLabelsQuery request, CancellationToken cancellationToken)
    {
        var labels = await _labelRepository.ListAsync(
            new { AccountId = (int)request.AccountId }, cancellationToken);

        return labels
            .Select(l => new LabelDto(l.Id, l.Title, l.Description, l.Color, l.ShowOnSidebar))
            .ToList()
            .AsReadOnly();
    }
}
