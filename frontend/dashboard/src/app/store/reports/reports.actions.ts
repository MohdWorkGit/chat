import { createActionGroup, props } from '@ngrx/store';
import { ReportMetric } from '@core/models/report.model';
import { ReportFilters } from '@core/services/report.service';
import { ApiError } from '@core/models/common.model';

export interface ReportSummary {
  conversationsCount: number;
  resolutionCount: number;
  avgFirstResponseTime: number;
  avgResolutionTime: number;
}

export const ReportsActions = createActionGroup({
  source: 'Reports',
  events: {
    'Load Summary': props<{ since: string; until: string }>(),
    'Load Summary Success': props<{ summary: ReportSummary }>(),
    'Load Summary Failure': props<{ error: ApiError }>(),

    'Load Conversation Metrics': props<{ filters: ReportFilters }>(),
    'Load Conversation Metrics Success': props<{ metrics: ReportMetric[] }>(),
    'Load Conversation Metrics Failure': props<{ error: ApiError }>(),

    'Load Agent Metrics': props<{ filters: ReportFilters }>(),
    'Load Agent Metrics Success': props<{ agentMetrics: ReportMetric[] }>(),
    'Load Agent Metrics Failure': props<{ error: ApiError }>(),

    'Set Filters': props<{ filters: Partial<ReportFilters> }>(),
  },
});
