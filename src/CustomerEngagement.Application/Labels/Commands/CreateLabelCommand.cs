using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Labels.Commands;

public record CreateLabelCommand(
    long AccountId,
    string Title,
    string? Description,
    string? Color,
    bool ShowOnSidebar = true) : IRequest<LabelDto>;

public class CreateLabelCommandHandler : IRequestHandler<CreateLabelCommand, LabelDto>
{
    private readonly IRepository<Label> _labelRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLabelCommandHandler(IRepository<Label> labelRepository, IUnitOfWork unitOfWork)
    {
        _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<LabelDto> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        var label = new Label
        {
            AccountId = (int)request.AccountId,
            Title = request.Title,
            Description = request.Description,
            Color = request.Color,
            ShowOnSidebar = request.ShowOnSidebar,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _labelRepository.AddAsync(label, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LabelDto(label.Id, label.Title, label.Description, label.Color, label.ShowOnSidebar);
    }
}
