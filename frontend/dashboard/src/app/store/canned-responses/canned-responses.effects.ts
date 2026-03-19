import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { CannedResponseService } from '@core/services/canned-response.service';
import { CannedResponsesActions } from './canned-responses.actions';
import { ApiError } from '@core/models/common.model';

export const loadCannedResponses$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CannedResponseService)) =>
    actions$.pipe(
      ofType(CannedResponsesActions.loadCannedResponses),
      switchMap(() =>
        svc.getAll().pipe(
          map((cannedResponses) => CannedResponsesActions.loadCannedResponsesSuccess({ cannedResponses })),
          catchError((error: ApiError) => of(CannedResponsesActions.loadCannedResponsesFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createCannedResponse$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CannedResponseService)) =>
    actions$.pipe(
      ofType(CannedResponsesActions.createCannedResponse),
      exhaustMap(({ data }) =>
        svc.create(data).pipe(
          map((cannedResponse) => CannedResponsesActions.createCannedResponseSuccess({ cannedResponse })),
          catchError((error: ApiError) => of(CannedResponsesActions.createCannedResponseFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateCannedResponse$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CannedResponseService)) =>
    actions$.pipe(
      ofType(CannedResponsesActions.updateCannedResponse),
      exhaustMap(({ id, data }) =>
        svc.update(id, data).pipe(
          map((cannedResponse) => CannedResponsesActions.updateCannedResponseSuccess({ cannedResponse })),
          catchError((error: ApiError) => of(CannedResponsesActions.updateCannedResponseFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteCannedResponse$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CannedResponseService)) =>
    actions$.pipe(
      ofType(CannedResponsesActions.deleteCannedResponse),
      exhaustMap(({ id }) =>
        svc.delete(id).pipe(
          map(() => CannedResponsesActions.deleteCannedResponseSuccess({ id })),
          catchError((error: ApiError) => of(CannedResponsesActions.deleteCannedResponseFailure({ error })))
        )
      )
    ),
  { functional: true }
);
