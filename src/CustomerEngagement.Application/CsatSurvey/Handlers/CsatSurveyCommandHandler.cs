using CustomerEngagement.Application.CsatSurvey.Commands;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CsatSurvey.Handlers;

public class SubmitCsatResponseHandler : IRequestHandler<SubmitCsatResponseCommand, CsatResponseResult>
{
    private readonly IRepository<CsatSurveyResponse> _csatRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitCsatResponseHandler(
        IRepository<CsatSurveyResponse> csatRepository,
        IUnitOfWork unitOfWork)
    {
        _csatRepository = csatRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CsatResponseResult> Handle(SubmitCsatResponseCommand request, CancellationToken cancellationToken)
    {
        var csatResponse = new CsatSurveyResponse
        {
            AccountId = request.AccountId,
            ConversationId = request.ConversationId,
            MessageId = request.MessageId,
            ContactId = request.ContactId,
            AssigneeId = request.AssigneeId,
            Rating = request.Rating,
            FeedbackText = request.FeedbackText,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _csatRepository.AddAsync(csatResponse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CsatResponseResult
        {
            Id = csatResponse.Id,
            ConversationId = csatResponse.ConversationId,
            Rating = csatResponse.Rating,
            FeedbackText = csatResponse.FeedbackText,
            CreatedAt = csatResponse.CreatedAt
        };
    }
}

public class UpdateCsatResponseHandler : IRequestHandler<UpdateCsatResponseCommand, CsatResponseResult>
{
    private readonly IRepository<CsatSurveyResponse> _csatRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCsatResponseHandler(
        IRepository<CsatSurveyResponse> csatRepository,
        IUnitOfWork unitOfWork)
    {
        _csatRepository = csatRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CsatResponseResult> Handle(UpdateCsatResponseCommand request, CancellationToken cancellationToken)
    {
        var csatResponse = await _csatRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"CSAT response {request.Id} not found.");

        if (request.Rating.HasValue)
            csatResponse.Rating = request.Rating.Value;

        if (request.FeedbackText is not null)
            csatResponse.FeedbackText = request.FeedbackText;

        csatResponse.UpdatedAt = DateTime.UtcNow;

        await _csatRepository.UpdateAsync(csatResponse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CsatResponseResult
        {
            Id = csatResponse.Id,
            ConversationId = csatResponse.ConversationId,
            Rating = csatResponse.Rating,
            FeedbackText = csatResponse.FeedbackText,
            CreatedAt = csatResponse.CreatedAt
        };
    }
}
