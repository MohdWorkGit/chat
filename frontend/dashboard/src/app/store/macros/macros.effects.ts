import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { MacroService } from '@core/services/macro.service';
import { MacrosActions } from './macros.actions';
import { ApiError } from '@core/models/common.model';

export const loadMacros$ = createEffect(
  (actions$ = inject(Actions), svc = inject(MacroService)) =>
    actions$.pipe(
      ofType(MacrosActions.loadMacros),
      switchMap(() =>
        svc.getAll().pipe(
          map((macros) => MacrosActions.loadMacrosSuccess({ macros })),
          catchError((error: ApiError) => of(MacrosActions.loadMacrosFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createMacro$ = createEffect(
  (actions$ = inject(Actions), svc = inject(MacroService)) =>
    actions$.pipe(
      ofType(MacrosActions.createMacro),
      exhaustMap(({ data }) =>
        svc.create(data).pipe(
          map((macro) => MacrosActions.createMacroSuccess({ macro })),
          catchError((error: ApiError) => of(MacrosActions.createMacroFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateMacro$ = createEffect(
  (actions$ = inject(Actions), svc = inject(MacroService)) =>
    actions$.pipe(
      ofType(MacrosActions.updateMacro),
      exhaustMap(({ id, data }) =>
        svc.update(id, data).pipe(
          map((macro) => MacrosActions.updateMacroSuccess({ macro })),
          catchError((error: ApiError) => of(MacrosActions.updateMacroFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteMacro$ = createEffect(
  (actions$ = inject(Actions), svc = inject(MacroService)) =>
    actions$.pipe(
      ofType(MacrosActions.deleteMacro),
      exhaustMap(({ id }) =>
        svc.delete(id).pipe(
          map(() => MacrosActions.deleteMacroSuccess({ id })),
          catchError((error: ApiError) => of(MacrosActions.deleteMacroFailure({ error })))
        )
      )
    ),
  { functional: true }
);
