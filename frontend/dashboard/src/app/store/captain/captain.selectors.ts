import { createFeatureSelector, createSelector } from '@ngrx/store';
import { CaptainState } from './captain.reducer';

export const selectCaptainState = createFeatureSelector<CaptainState>('captain');

export const selectAssistants = createSelector(selectCaptainState, (state) => state.assistants);

export const selectDocuments = createSelector(selectCaptainState, (state) => state.documents);

export const selectCopilotMessages = createSelector(selectCaptainState, (state) => state.copilotMessages);

export const selectLoading = createSelector(selectCaptainState, (state) => state.loading);

export const selectError = createSelector(selectCaptainState, (state) => state.error);
