using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Reporting;
using MediatR;

namespace CustomerEngagement.Application.Reports.Queries;

public record GetConversationReportQuery(long AccountId, DateTime Since, DateTime Until, string? GroupBy) : IRequest<object>;

public record GetAgentReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public record GetInboxReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public record GetTeamReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public record GetLabelReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public record GetSummaryReportQuery(long AccountId, DateTime Since, DateTime Until) : IRequest<object>;

public class GetConversationReportQueryHandler : IRequestHandler<GetConversationReportQuery, object>
{
    private readonly IReportBuilder _reportBuilder;

    public GetConversationReportQueryHandler(IReportBuilder reportBuilder)
    {
        _reportBuilder = reportBuilder;
    }

    public async Task<object> Handle(GetConversationReportQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportFilterDto
        {
            Since = request.Since,
            Until = request.Until,
            GroupBy = request.GroupBy ?? "day"
        };

        return await _reportBuilder.GetConversationReportAsync((int)request.AccountId, filter, cancellationToken);
    }
}

public class GetAgentReportQueryHandler : IRequestHandler<GetAgentReportQuery, object>
{
    private readonly IReportBuilder _reportBuilder;

    public GetAgentReportQueryHandler(IReportBuilder reportBuilder)
    {
        _reportBuilder = reportBuilder;
    }

    public async Task<object> Handle(GetAgentReportQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportFilterDto
        {
            Since = request.Since,
            Until = request.Until
        };

        return await _reportBuilder.GetAgentReportAsync((int)request.AccountId, filter, cancellationToken);
    }
}

public class GetInboxReportQueryHandler : IRequestHandler<GetInboxReportQuery, object>
{
    private readonly IReportBuilder _reportBuilder;

    public GetInboxReportQueryHandler(IReportBuilder reportBuilder)
    {
        _reportBuilder = reportBuilder;
    }

    public async Task<object> Handle(GetInboxReportQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportFilterDto
        {
            Since = request.Since,
            Until = request.Until
        };

        return await _reportBuilder.GetInboxReportAsync((int)request.AccountId, filter, cancellationToken);
    }
}

public class GetTeamReportQueryHandler : IRequestHandler<GetTeamReportQuery, object>
{
    private readonly IReportBuilder _reportBuilder;

    public GetTeamReportQueryHandler(IReportBuilder reportBuilder)
    {
        _reportBuilder = reportBuilder;
    }

    public async Task<object> Handle(GetTeamReportQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportFilterDto
        {
            Since = request.Since,
            Until = request.Until
        };

        return await _reportBuilder.GetTeamReportAsync((int)request.AccountId, filter, cancellationToken);
    }
}

public class GetLabelReportQueryHandler : IRequestHandler<GetLabelReportQuery, object>
{
    private readonly IReportBuilder _reportBuilder;

    public GetLabelReportQueryHandler(IReportBuilder reportBuilder)
    {
        _reportBuilder = reportBuilder;
    }

    public async Task<object> Handle(GetLabelReportQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportFilterDto
        {
            Since = request.Since,
            Until = request.Until
        };

        return await _reportBuilder.GetLabelReportAsync((int)request.AccountId, filter, cancellationToken);
    }
}

public class GetSummaryReportQueryHandler : IRequestHandler<GetSummaryReportQuery, object>
{
    private readonly IReportBuilder _reportBuilder;

    public GetSummaryReportQueryHandler(IReportBuilder reportBuilder)
    {
        _reportBuilder = reportBuilder;
    }

    public async Task<object> Handle(GetSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportFilterDto
        {
            Since = request.Since,
            Until = request.Until
        };

        return await _reportBuilder.GetSummaryAsync((int)request.AccountId, filter, cancellationToken);
    }
}
