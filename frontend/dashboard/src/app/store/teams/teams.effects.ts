import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { TeamService } from '@core/services/team.service';
import { TeamsActions } from './teams.actions';
import { ApiError } from '@core/models/common.model';

export const loadTeams$ = createEffect(
  (actions$ = inject(Actions), teamService = inject(TeamService)) =>
    actions$.pipe(
      ofType(TeamsActions.loadTeams),
      switchMap(() =>
        teamService.getAll().pipe(
          map((teams) => TeamsActions.loadTeamsSuccess({ teams })),
          catchError((error: ApiError) => of(TeamsActions.loadTeamsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadTeam$ = createEffect(
  (actions$ = inject(Actions), teamService = inject(TeamService)) =>
    actions$.pipe(
      ofType(TeamsActions.loadTeam),
      switchMap(({ id }) =>
        teamService.getById(id).pipe(
          map((team) => TeamsActions.loadTeamSuccess({ team })),
          catchError((error: ApiError) => of(TeamsActions.loadTeamFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createTeam$ = createEffect(
  (actions$ = inject(Actions), teamService = inject(TeamService)) =>
    actions$.pipe(
      ofType(TeamsActions.createTeam),
      exhaustMap(({ data }) =>
        teamService.create(data).pipe(
          map((team) => TeamsActions.createTeamSuccess({ team })),
          catchError((error: ApiError) => of(TeamsActions.createTeamFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateTeam$ = createEffect(
  (actions$ = inject(Actions), teamService = inject(TeamService)) =>
    actions$.pipe(
      ofType(TeamsActions.updateTeam),
      exhaustMap(({ id, data }) =>
        teamService.update(id, data).pipe(
          map((team) => TeamsActions.updateTeamSuccess({ team })),
          catchError((error: ApiError) => of(TeamsActions.updateTeamFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteTeam$ = createEffect(
  (actions$ = inject(Actions), teamService = inject(TeamService)) =>
    actions$.pipe(
      ofType(TeamsActions.deleteTeam),
      exhaustMap(({ id }) =>
        teamService.delete(id).pipe(
          map(() => TeamsActions.deleteTeamSuccess({ id })),
          catchError((error: ApiError) => of(TeamsActions.deleteTeamFailure({ error })))
        )
      )
    ),
  { functional: true }
);
