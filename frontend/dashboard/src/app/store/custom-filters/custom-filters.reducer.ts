import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { CustomFilter } from '@core/models/custom-filter.model';
import { ApiError } from '@core/models/common.model';
import { CustomFiltersActions } from './custom-filters.actions';

export interface CustomFiltersState extends EntityState<CustomFilter> {
  selectedFilterId: number | null;
  loading: boolean;
  error: ApiError | null;
}

export const customFiltersAdapter: EntityAdapter<CustomFilter> = createEntityAdapter<CustomFilter>({
  selectId: (filter) => filter.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialCustomFiltersState: CustomFiltersState = customFiltersAdapter.getInitialState({
  selectedFilterId: null,
  loading: false,
  error: null,
});

export const customFiltersReducer = createReducer(
  initialCustomFiltersState,

  on(CustomFiltersActions.loadCustomFilters, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CustomFiltersActions.loadCustomFiltersSuccess, (state, { filters }) =>
    customFiltersAdapter.setAll(filters, {
      ...state,
      loading: false,
    })
  ),

  on(CustomFiltersActions.loadCustomFiltersFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(CustomFiltersActions.createCustomFilterSuccess, (state, { filter }) =>
    customFiltersAdapter.addOne(filter, state)
  ),

  on(CustomFiltersActions.updateCustomFilterSuccess, (state, { filter }) =>
    customFiltersAdapter.updateOne({ id: filter.id, changes: filter }, state)
  ),

  on(CustomFiltersActions.deleteCustomFilterSuccess, (state, { id }) =>
    customFiltersAdapter.removeOne(id, state)
  ),

  on(CustomFiltersActions.selectCustomFilter, (state, { id }) => ({
    ...state,
    selectedFilterId: id,
  })),

  on(CustomFiltersActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
