import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { CustomAttributeService } from '@core/services/custom-attribute.service';
import { CustomAttributesActions } from './custom-attributes.actions';
import { ApiError } from '@core/models/common.model';

export const loadCustomAttributes$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CustomAttributeService)) =>
    actions$.pipe(
      ofType(CustomAttributesActions.loadCustomAttributes),
      switchMap(() =>
        svc.getAll().pipe(
          map((customAttributes) => CustomAttributesActions.loadCustomAttributesSuccess({ customAttributes })),
          catchError((error: ApiError) => of(CustomAttributesActions.loadCustomAttributesFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createCustomAttribute$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CustomAttributeService)) =>
    actions$.pipe(
      ofType(CustomAttributesActions.createCustomAttribute),
      exhaustMap(({ data }) =>
        svc.create(data).pipe(
          map((customAttribute) => CustomAttributesActions.createCustomAttributeSuccess({ customAttribute })),
          catchError((error: ApiError) => of(CustomAttributesActions.createCustomAttributeFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateCustomAttribute$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CustomAttributeService)) =>
    actions$.pipe(
      ofType(CustomAttributesActions.updateCustomAttribute),
      exhaustMap(({ id, data }) =>
        svc.update(id, data).pipe(
          map((customAttribute) => CustomAttributesActions.updateCustomAttributeSuccess({ customAttribute })),
          catchError((error: ApiError) => of(CustomAttributesActions.updateCustomAttributeFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteCustomAttribute$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CustomAttributeService)) =>
    actions$.pipe(
      ofType(CustomAttributesActions.deleteCustomAttribute),
      exhaustMap(({ id }) =>
        svc.delete(id).pipe(
          map(() => CustomAttributesActions.deleteCustomAttributeSuccess({ id })),
          catchError((error: ApiError) => of(CustomAttributesActions.deleteCustomAttributeFailure({ error })))
        )
      )
    ),
  { functional: true }
);
