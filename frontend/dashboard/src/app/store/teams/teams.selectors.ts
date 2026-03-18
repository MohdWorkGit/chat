import { createFeatureSelector, createSelector } from '@ngrx/store';
import { TeamsState, teamsAdapter } from './teams.reducer';

export const selectTeamsState = createFeatureSelector<TeamsState>('teams');

const { selectAll, selectEntities } = teamsAdapter.getSelectors();

export const selectAllTeams = createSelector(selectTeamsState, selectAll);

export const selectTeamEntities = createSelector(selectTeamsState, selectEntities);

export const selectSelectedTeamId = createSelector(selectTeamsState, (state) => state.selectedTeamId);

export const selectSelectedTeam = createSelector(
  selectTeamEntities,
  selectSelectedTeamId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);

export const selectTeamsLoading = createSelector(selectTeamsState, (state) => state.loading);

export const selectTeamsError = createSelector(selectTeamsState, (state) => state.error);
