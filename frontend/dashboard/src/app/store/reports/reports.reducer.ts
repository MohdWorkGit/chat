import { createReducer, on } from '@ngrx/store';
import { ReportMetric } from '@core/models/report.model';
import { ReportFilters } from '@core/services/report.service';
import { ApiError } from '@core/models/common.model';
import { ReportsActions, ReportSummary } from './reports.actions';

export interface ReportsState {
  summary: ReportSummary | null;
  metrics: ReportMetric[];
  agentMetrics: ReportMetric[];
  filters: ReportFilters;
  loading: boolean;
  error: ApiError | null;
}

export const initialReportsState: ReportsState = {
  summary: null,
  metrics: [],
  agentMetrics: [],
  filters: {
    type: 'account',
    since: '',
    until: '',
    groupBy: 'day',
  },
  loading: false,
  error: null,
};

export const reportsReducer = createReducer(
  initialReportsState,

  on(ReportsActions.loadSummary, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(ReportsActions.loadSummarySuccess, (state, { summary }) => ({
    ...state,
    summary,
    loading: false,
  })),

  on(ReportsActions.loadSummaryFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(ReportsActions.loadConversationMetrics, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(ReportsActions.loadConversationMetricsSuccess, (state, { metrics }) => ({
    ...state,
    metrics,
    loading: false,
  })),

  on(ReportsActions.loadConversationMetricsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(ReportsActions.loadAgentMetrics, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(ReportsActions.loadAgentMetricsSuccess, (state, { agentMetrics }) => ({
    ...state,
    agentMetrics,
    loading: false,
  })),

  on(ReportsActions.loadAgentMetricsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(ReportsActions.setFilters, (state, { filters }) => ({
    ...state,
    filters: { ...state.filters, ...filters },
  }))
);
