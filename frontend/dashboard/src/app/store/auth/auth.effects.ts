import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, tap } from 'rxjs';
import { AuthService } from '@core/services/auth.service';
import { SignalRService } from '@core/services/signalr.service';
import { AuthActions } from './auth.actions';
import { ApiError } from '@core/models/common.model';

export const login$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) =>
    actions$.pipe(
      ofType(AuthActions.login),
      exhaustMap(({ credentials }) =>
        authService.login(credentials).pipe(
          map((response) => AuthActions.loginSuccess({ response })),
          catchError((error: ApiError) => of(AuthActions.loginFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loginSuccess$ = createEffect(
  (actions$ = inject(Actions), router = inject(Router), signalrService = inject(SignalRService)) =>
    actions$.pipe(
      ofType(AuthActions.loginSuccess),
      tap(({ response }) => {
        signalrService.connect().then(() => {
          signalrService.joinAccountGroup(response.user.accountId);
        });
        router.navigate(['/']);
      })
    ),
  { functional: true, dispatch: false }
);

export const register$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) =>
    actions$.pipe(
      ofType(AuthActions.register),
      exhaustMap(({ data }) =>
        authService.register(data).pipe(
          map((response) => AuthActions.registerSuccess({ response })),
          catchError((error: ApiError) => of(AuthActions.registerFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const registerSuccess$ = createEffect(
  (actions$ = inject(Actions), router = inject(Router)) =>
    actions$.pipe(
      ofType(AuthActions.registerSuccess),
      tap(() => {
        router.navigate(['/']);
      })
    ),
  { functional: true, dispatch: false }
);

export const logout$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService), router = inject(Router), signalrService = inject(SignalRService)) =>
    actions$.pipe(
      ofType(AuthActions.logout),
      tap(() => {
        signalrService.disconnect();
        authService.logout();
        router.navigate(['/auth/login']);
      }),
      map(() => AuthActions.logoutComplete())
    ),
  { functional: true }
);

export const loadCurrentUser$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) =>
    actions$.pipe(
      ofType(AuthActions.loadCurrentUser),
      exhaustMap(() =>
        authService.getCurrentUser().pipe(
          map((user) => AuthActions.loadCurrentUserSuccess({ user })),
          catchError((error: ApiError) => of(AuthActions.loadCurrentUserFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const forgotPassword$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) =>
    actions$.pipe(
      ofType(AuthActions.forgotPassword),
      exhaustMap(({ email }) =>
        authService.forgotPassword(email).pipe(
          map(() => AuthActions.forgotPasswordSuccess()),
          catchError((error: ApiError) => of(AuthActions.forgotPasswordFailure({ error })))
        )
      )
    ),
  { functional: true }
);
