import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Team } from '@core/models/team.model';
import { ApiError } from '@core/models/common.model';
import { TeamsActions } from './teams.actions';

export interface TeamsState extends EntityState<Team> {
  selectedTeamId: number | null;
  loading: boolean;
  error: ApiError | null;
}

export const teamsAdapter: EntityAdapter<Team> = createEntityAdapter<Team>({
  selectId: (team) => team.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialTeamsState: TeamsState = teamsAdapter.getInitialState({
  selectedTeamId: null,
  loading: false,
  error: null,
});

export const teamsReducer = createReducer(
  initialTeamsState,

  on(TeamsActions.loadTeams, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(TeamsActions.loadTeamsSuccess, (state, { teams }) =>
    teamsAdapter.setAll(teams, {
      ...state,
      loading: false,
    })
  ),

  on(TeamsActions.loadTeamsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(TeamsActions.loadTeamSuccess, (state, { team }) =>
    teamsAdapter.upsertOne(team, state)
  ),

  on(TeamsActions.createTeamSuccess, (state, { team }) =>
    teamsAdapter.addOne(team, state)
  ),

  on(TeamsActions.updateTeamSuccess, (state, { team }) =>
    teamsAdapter.updateOne({ id: team.id, changes: team }, state)
  ),

  on(TeamsActions.deleteTeamSuccess, (state, { id }) =>
    teamsAdapter.removeOne(id, state)
  ),

  on(TeamsActions.selectTeam, (state, { id }) => ({
    ...state,
    selectedTeamId: id,
  })),

  on(TeamsActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
