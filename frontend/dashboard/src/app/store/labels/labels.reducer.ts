import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Label } from '@core/models/label.model';
import { ApiError } from '@core/models/common.model';
import { LabelsActions } from './labels.actions';

export interface LabelsState extends EntityState<Label> {
  selectedLabelId: number | null;
  loading: boolean;
  error: ApiError | null;
}

export const labelsAdapter: EntityAdapter<Label> = createEntityAdapter<Label>({
  selectId: (label) => label.id,
  sortComparer: (a, b) => a.title.localeCompare(b.title),
});

export const initialLabelsState: LabelsState = labelsAdapter.getInitialState({
  selectedLabelId: null,
  loading: false,
  error: null,
});

export const labelsReducer = createReducer(
  initialLabelsState,

  on(LabelsActions.loadLabels, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(LabelsActions.loadLabelsSuccess, (state, { labels }) =>
    labelsAdapter.setAll(labels, {
      ...state,
      loading: false,
    })
  ),

  on(LabelsActions.loadLabelsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(LabelsActions.loadLabelSuccess, (state, { label }) =>
    labelsAdapter.upsertOne(label, state)
  ),

  on(LabelsActions.createLabelSuccess, (state, { label }) =>
    labelsAdapter.addOne(label, state)
  ),

  on(LabelsActions.updateLabelSuccess, (state, { label }) =>
    labelsAdapter.updateOne({ id: label.id, changes: label }, state)
  ),

  on(LabelsActions.deleteLabelSuccess, (state, { id }) =>
    labelsAdapter.removeOne(id, state)
  ),

  on(LabelsActions.selectLabel, (state, { id }) => ({
    ...state,
    selectedLabelId: id,
  })),

  on(LabelsActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
