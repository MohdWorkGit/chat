import { createFeatureSelector, createSelector } from '@ngrx/store';
import { LabelsState, labelsAdapter } from './labels.reducer';

export const selectLabelsState = createFeatureSelector<LabelsState>('labels');

const { selectAll, selectEntities } = labelsAdapter.getSelectors();

export const selectAllLabels = createSelector(selectLabelsState, selectAll);

export const selectLabelEntities = createSelector(selectLabelsState, selectEntities);

export const selectSelectedLabelId = createSelector(selectLabelsState, (state) => state.selectedLabelId);

export const selectSelectedLabel = createSelector(
  selectLabelEntities,
  selectSelectedLabelId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);

export const selectLabelsLoading = createSelector(selectLabelsState, (state) => state.loading);

export const selectLabelsError = createSelector(selectLabelsState, (state) => state.error);
