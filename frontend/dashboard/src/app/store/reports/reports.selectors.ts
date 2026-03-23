import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ReportsState } from './reports.reducer';

export const selectReportsState = createFeatureSelector<ReportsState>('reports');

export const selectReportSummary = createSelector(selectReportsState, (state) => state.summary);

export const selectReportMetrics = createSelector(selectReportsState, (state) => state.metrics);

export const selectAgentMetrics = createSelector(selectReportsState, (state) => state.agentMetrics);

export const selectReportFilters = createSelector(selectReportsState, (state) => state.filters);

export const selectReportsLoading = createSelector(selectReportsState, (state) => state.loading);

export const selectReportsError = createSelector(selectReportsState, (state) => state.error);
