import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AutomationsState, automationsAdapter } from './automations.reducer';

export const selectAutomationsState = createFeatureSelector<AutomationsState>('automations');

const { selectAll, selectEntities } = automationsAdapter.getSelectors();

export const selectAllAutomations = createSelector(selectAutomationsState, selectAll);
export const selectAutomationEntities = createSelector(selectAutomationsState, selectEntities);
export const selectSelectedAutomationId = createSelector(selectAutomationsState, (state) => state.selectedAutomationId);
export const selectSelectedAutomation = createSelector(
  selectAutomationEntities,
  selectSelectedAutomationId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);
export const selectAutomationsLoading = createSelector(selectAutomationsState, (state) => state.loading);
export const selectAutomationsError = createSelector(selectAutomationsState, (state) => state.error);
