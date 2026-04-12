using CustomerEngagement.Application.DTOs;

namespace CustomerEngagement.Application.Services.Reporting;

public interface IReportBuilder
{
    Task<ReportDto> GetConversationReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ReportDto> GetAgentReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ReportDto> GetInboxReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ReportDto> GetTeamReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ReportDto> GetLabelReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ReportSummaryDto> GetSummaryAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<object> GetTrafficReportAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<object> GetBotMetricsAsync(int accountId, ReportFilterDto filter, CancellationToken cancellationToken = default);
}
