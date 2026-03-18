import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { AutomationService } from '@core/services/automation.service';
import { AutomationsActions } from './automations.actions';
import { ApiError } from '@core/models/common.model';

export const loadAutomations$ = createEffect(
  (actions$ = inject(Actions), svc = inject(AutomationService)) =>
    actions$.pipe(
      ofType(AutomationsActions.loadAutomations),
      switchMap(() =>
        svc.getAll().pipe(
          map((automations) => AutomationsActions.loadAutomationsSuccess({ automations })),
          catchError((error: ApiError) => of(AutomationsActions.loadAutomationsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createAutomation$ = createEffect(
  (actions$ = inject(Actions), svc = inject(AutomationService)) =>
    actions$.pipe(
      ofType(AutomationsActions.createAutomation),
      exhaustMap(({ data }) =>
        svc.create(data).pipe(
          map((automation) => AutomationsActions.createAutomationSuccess({ automation })),
          catchError((error: ApiError) => of(AutomationsActions.createAutomationFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateAutomation$ = createEffect(
  (actions$ = inject(Actions), svc = inject(AutomationService)) =>
    actions$.pipe(
      ofType(AutomationsActions.updateAutomation),
      exhaustMap(({ id, data }) =>
        svc.update(id, data).pipe(
          map((automation) => AutomationsActions.updateAutomationSuccess({ automation })),
          catchError((error: ApiError) => of(AutomationsActions.updateAutomationFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteAutomation$ = createEffect(
  (actions$ = inject(Actions), svc = inject(AutomationService)) =>
    actions$.pipe(
      ofType(AutomationsActions.deleteAutomation),
      exhaustMap(({ id }) =>
        svc.delete(id).pipe(
          map(() => AutomationsActions.deleteAutomationSuccess({ id })),
          catchError((error: ApiError) => of(AutomationsActions.deleteAutomationFailure({ error })))
        )
      )
    ),
  { functional: true }
);
