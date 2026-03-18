using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Labels.Commands;

public record DeleteLabelCommand(long AccountId, long Id) : IRequest;

public class DeleteLabelCommandHandler : IRequestHandler<DeleteLabelCommand>
{
    private readonly IRepository<Label> _labelRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLabelCommandHandler(IRepository<Label> labelRepository, IUnitOfWork unitOfWork)
    {
        _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(DeleteLabelCommand request, CancellationToken cancellationToken)
    {
        var label = await _labelRepository.GetByIdAsync((int)request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Label {request.Id} not found.");

        await _labelRepository.DeleteAsync(label, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
