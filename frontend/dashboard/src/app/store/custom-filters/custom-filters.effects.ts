import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { CustomFilterService } from '@core/services/custom-filter.service';
import { CustomFiltersActions } from './custom-filters.actions';
import { ApiError } from '@core/models/common.model';

export const loadCustomFilters$ = createEffect(
  (actions$ = inject(Actions), filterService = inject(CustomFilterService)) =>
    actions$.pipe(
      ofType(CustomFiltersActions.loadCustomFilters),
      switchMap(() =>
        filterService.getAll().pipe(
          map((filters) => CustomFiltersActions.loadCustomFiltersSuccess({ filters })),
          catchError((error: ApiError) => of(CustomFiltersActions.loadCustomFiltersFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createCustomFilter$ = createEffect(
  (actions$ = inject(Actions), filterService = inject(CustomFilterService)) =>
    actions$.pipe(
      ofType(CustomFiltersActions.createCustomFilter),
      exhaustMap(({ data }) =>
        filterService.create(data).pipe(
          map((filter) => CustomFiltersActions.createCustomFilterSuccess({ filter })),
          catchError((error: ApiError) => of(CustomFiltersActions.createCustomFilterFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateCustomFilter$ = createEffect(
  (actions$ = inject(Actions), filterService = inject(CustomFilterService)) =>
    actions$.pipe(
      ofType(CustomFiltersActions.updateCustomFilter),
      exhaustMap(({ id, data }) =>
        filterService.update(id, data).pipe(
          map((filter) => CustomFiltersActions.updateCustomFilterSuccess({ filter })),
          catchError((error: ApiError) => of(CustomFiltersActions.updateCustomFilterFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteCustomFilter$ = createEffect(
  (actions$ = inject(Actions), filterService = inject(CustomFilterService)) =>
    actions$.pipe(
      ofType(CustomFiltersActions.deleteCustomFilter),
      exhaustMap(({ id }) =>
        filterService.delete(id).pipe(
          map(() => CustomFiltersActions.deleteCustomFilterSuccess({ id })),
          catchError((error: ApiError) => of(CustomFiltersActions.deleteCustomFilterFailure({ error })))
        )
      )
    ),
  { functional: true }
);
