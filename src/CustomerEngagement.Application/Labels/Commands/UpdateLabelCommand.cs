using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Labels.Commands;

public record UpdateLabelCommand(
    long AccountId,
    long Id,
    string? Title,
    string? Description,
    string? Color,
    bool? ShowOnSidebar) : IRequest<LabelDto>;

public class UpdateLabelCommandHandler : IRequestHandler<UpdateLabelCommand, LabelDto>
{
    private readonly IRepository<Label> _labelRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLabelCommandHandler(IRepository<Label> labelRepository, IUnitOfWork unitOfWork)
    {
        _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<LabelDto> Handle(UpdateLabelCommand request, CancellationToken cancellationToken)
    {
        var label = await _labelRepository.GetByIdAsync((int)request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Label {request.Id} not found.");

        if (request.Title is not null) label.Title = request.Title;
        if (request.Description is not null) label.Description = request.Description;
        if (request.Color is not null) label.Color = request.Color;
        if (request.ShowOnSidebar.HasValue) label.ShowOnSidebar = request.ShowOnSidebar.Value;
        label.UpdatedAt = DateTime.UtcNow;

        await _labelRepository.UpdateAsync(label, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LabelDto(label.Id, label.Title, label.Description, label.Color, label.ShowOnSidebar);
    }
}
