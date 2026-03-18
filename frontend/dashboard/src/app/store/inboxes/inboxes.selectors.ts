import { createFeatureSelector, createSelector } from '@ngrx/store';
import { InboxesState, inboxesAdapter } from './inboxes.reducer';

export const selectInboxesState = createFeatureSelector<InboxesState>('inboxes');

const { selectAll, selectEntities } = inboxesAdapter.getSelectors();

export const selectAllInboxes = createSelector(selectInboxesState, selectAll);

export const selectInboxEntities = createSelector(selectInboxesState, selectEntities);

export const selectSelectedInboxId = createSelector(selectInboxesState, (state) => state.selectedInboxId);

export const selectSelectedInbox = createSelector(
  selectInboxEntities,
  selectSelectedInboxId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);

export const selectInboxesLoading = createSelector(selectInboxesState, (state) => state.loading);

export const selectInboxesError = createSelector(selectInboxesState, (state) => state.error);
