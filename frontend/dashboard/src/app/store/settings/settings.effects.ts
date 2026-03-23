import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { SettingsService } from '@core/services/settings.service';
import { SettingsActions } from './settings.actions';
import { ApiError } from '@core/models/common.model';

export const loadAccountSettings$ = createEffect(
  (actions$ = inject(Actions), settingsService = inject(SettingsService)) =>
    actions$.pipe(
      ofType(SettingsActions.loadAccountSettings),
      switchMap(() =>
        settingsService.getAccountSettings().pipe(
          map((accountSettings) => SettingsActions.loadAccountSettingsSuccess({ accountSettings })),
          catchError((error: ApiError) => of(SettingsActions.loadAccountSettingsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateAccountSettings$ = createEffect(
  (actions$ = inject(Actions), settingsService = inject(SettingsService)) =>
    actions$.pipe(
      ofType(SettingsActions.updateAccountSettings),
      exhaustMap(({ data }) =>
        settingsService.updateAccountSettings(data).pipe(
          map((accountSettings) => SettingsActions.updateAccountSettingsSuccess({ accountSettings })),
          catchError((error: ApiError) => of(SettingsActions.updateAccountSettingsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadProfile$ = createEffect(
  (actions$ = inject(Actions), settingsService = inject(SettingsService)) =>
    actions$.pipe(
      ofType(SettingsActions.loadProfile),
      switchMap(() =>
        settingsService.getProfile().pipe(
          map((profile) => SettingsActions.loadProfileSuccess({ profile })),
          catchError((error: ApiError) => of(SettingsActions.loadProfileFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateProfile$ = createEffect(
  (actions$ = inject(Actions), settingsService = inject(SettingsService)) =>
    actions$.pipe(
      ofType(SettingsActions.updateProfile),
      exhaustMap(({ data }) =>
        settingsService.updateProfile(data).pipe(
          map((profile) => SettingsActions.updateProfileSuccess({ profile })),
          catchError((error: ApiError) => of(SettingsActions.updateProfileFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateNotificationPreferences$ = createEffect(
  (actions$ = inject(Actions), settingsService = inject(SettingsService)) =>
    actions$.pipe(
      ofType(SettingsActions.updateNotificationPreferences),
      exhaustMap(({ prefs }) =>
        settingsService.updateNotificationPreferences(prefs).pipe(
          map((prefs) => SettingsActions.updateNotificationPreferencesSuccess({ prefs })),
          catchError((error: ApiError) => of(SettingsActions.updateNotificationPreferencesFailure({ error })))
        )
      )
    ),
  { functional: true }
);
