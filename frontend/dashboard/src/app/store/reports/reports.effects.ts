import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { ReportService } from '@core/services/report.service';
import { ReportsActions } from './reports.actions';
import { ApiError } from '@core/models/common.model';

export const loadSummary$ = createEffect(
  (actions$ = inject(Actions), reportService = inject(ReportService)) =>
    actions$.pipe(
      ofType(ReportsActions.loadSummary),
      switchMap(({ since, until }) =>
        reportService.getSummary(since, until).pipe(
          map((result) =>
            ReportsActions.loadSummarySuccess({
              summary: {
                conversationsCount: result.conversationsCount,
                resolutionCount: result.resolutionCount,
                avgFirstResponseTime: result.avgFirstResponseTime,
                avgResolutionTime: result.avgResolutionTime,
              },
            })
          ),
          catchError((error: ApiError) => of(ReportsActions.loadSummaryFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadConversationMetrics$ = createEffect(
  (actions$ = inject(Actions), reportService = inject(ReportService)) =>
    actions$.pipe(
      ofType(ReportsActions.loadConversationMetrics),
      switchMap(({ filters }) =>
        reportService.getConversationMetrics(filters).pipe(
          map((metrics) => ReportsActions.loadConversationMetricsSuccess({ metrics })),
          catchError((error: ApiError) => of(ReportsActions.loadConversationMetricsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadAgentMetrics$ = createEffect(
  (actions$ = inject(Actions), reportService = inject(ReportService)) =>
    actions$.pipe(
      ofType(ReportsActions.loadAgentMetrics),
      switchMap(({ filters }) =>
        reportService.getAgentMetrics(filters).pipe(
          map((agentMetrics) => ReportsActions.loadAgentMetricsSuccess({ agentMetrics })),
          catchError((error: ApiError) => of(ReportsActions.loadAgentMetricsFailure({ error })))
        )
      )
    ),
  { functional: true }
);
