import { createFeatureSelector, createSelector } from '@ngrx/store';
import { CannedResponsesState, cannedResponsesAdapter } from './canned-responses.reducer';

export const selectCannedResponsesState = createFeatureSelector<CannedResponsesState>('cannedResponses');

const { selectAll, selectEntities } = cannedResponsesAdapter.getSelectors();

export const selectAllCannedResponses = createSelector(selectCannedResponsesState, selectAll);
export const selectCannedResponseEntities = createSelector(selectCannedResponsesState, selectEntities);
export const selectCannedResponsesLoading = createSelector(selectCannedResponsesState, (state) => state.loading);
export const selectCannedResponsesError = createSelector(selectCannedResponsesState, (state) => state.error);
