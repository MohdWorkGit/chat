import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { CannedResponse } from '@core/models/canned-response.model';
import { ApiError } from '@core/models/common.model';
import { CannedResponsesActions } from './canned-responses.actions';

export interface CannedResponsesState extends EntityState<CannedResponse> {
  loading: boolean;
  error: ApiError | null;
}

export const cannedResponsesAdapter: EntityAdapter<CannedResponse> = createEntityAdapter<CannedResponse>({
  selectId: (cr) => cr.id,
  sortComparer: (a, b) => a.shortCode.localeCompare(b.shortCode),
});

export const initialCannedResponsesState: CannedResponsesState = cannedResponsesAdapter.getInitialState({
  loading: false,
  error: null,
});

export const cannedResponsesReducer = createReducer(
  initialCannedResponsesState,

  on(CannedResponsesActions.loadCannedResponses, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CannedResponsesActions.loadCannedResponsesSuccess, (state, { cannedResponses }) =>
    cannedResponsesAdapter.setAll(cannedResponses, { ...state, loading: false })
  ),

  on(CannedResponsesActions.loadCannedResponsesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(CannedResponsesActions.createCannedResponseSuccess, (state, { cannedResponse }) =>
    cannedResponsesAdapter.addOne(cannedResponse, state)
  ),

  on(CannedResponsesActions.updateCannedResponseSuccess, (state, { cannedResponse }) =>
    cannedResponsesAdapter.updateOne({ id: cannedResponse.id, changes: cannedResponse }, state)
  ),

  on(CannedResponsesActions.deleteCannedResponseSuccess, (state, { id }) =>
    cannedResponsesAdapter.removeOne(id, state)
  ),

  on(CannedResponsesActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
