import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { LabelService } from '@core/services/label.service';
import { LabelsActions } from './labels.actions';
import { ApiError } from '@core/models/common.model';

export const loadLabels$ = createEffect(
  (actions$ = inject(Actions), labelService = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelsActions.loadLabels),
      switchMap(() =>
        labelService.getAll().pipe(
          map((labels) => LabelsActions.loadLabelsSuccess({ labels })),
          catchError((error: ApiError) => of(LabelsActions.loadLabelsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadLabel$ = createEffect(
  (actions$ = inject(Actions), labelService = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelsActions.loadLabel),
      switchMap(({ id }) =>
        labelService.getById(id).pipe(
          map((label) => LabelsActions.loadLabelSuccess({ label })),
          catchError((error: ApiError) => of(LabelsActions.loadLabelFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createLabel$ = createEffect(
  (actions$ = inject(Actions), labelService = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelsActions.createLabel),
      exhaustMap(({ data }) =>
        labelService.create(data).pipe(
          map((label) => LabelsActions.createLabelSuccess({ label })),
          catchError((error: ApiError) => of(LabelsActions.createLabelFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateLabel$ = createEffect(
  (actions$ = inject(Actions), labelService = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelsActions.updateLabel),
      exhaustMap(({ id, data }) =>
        labelService.update(id, data).pipe(
          map((label) => LabelsActions.updateLabelSuccess({ label })),
          catchError((error: ApiError) => of(LabelsActions.updateLabelFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteLabel$ = createEffect(
  (actions$ = inject(Actions), labelService = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelsActions.deleteLabel),
      exhaustMap(({ id }) =>
        labelService.delete(id).pipe(
          map(() => LabelsActions.deleteLabelSuccess({ id })),
          catchError((error: ApiError) => of(LabelsActions.deleteLabelFailure({ error })))
        )
      )
    ),
  { functional: true }
);
