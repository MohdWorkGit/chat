using MediatR;

namespace CustomerEngagement.Application.Reports.Queries;

public record GetConversationReportQuery(long AccountId, DateTime Since, DateTime Until, string? GroupBy) : IRequest<object>;

public record GetAgentReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public record GetInboxReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public record GetTeamReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public record GetLabelReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public record GetSummaryReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;
