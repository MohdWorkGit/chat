using CustomerEngagement.Application.DTOs;
using MediatR;

namespace CustomerEngagement.Application.Labels.Queries;

public record GetLabelByIdQuery(long AccountId, long Id) : IRequest<LabelDto?>;
