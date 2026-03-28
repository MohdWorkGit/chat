import { createFeatureSelector, createSelector } from '@ngrx/store';
import { CustomFiltersState, customFiltersAdapter } from './custom-filters.reducer';

export const selectCustomFiltersState = createFeatureSelector<CustomFiltersState>('customFilters');

const { selectAll, selectEntities } = customFiltersAdapter.getSelectors();

export const selectAllCustomFilters = createSelector(selectCustomFiltersState, selectAll);

export const selectCustomFilterEntities = createSelector(selectCustomFiltersState, selectEntities);

export const selectSelectedFilterId = createSelector(selectCustomFiltersState, (state) => state.selectedFilterId);

export const selectSelectedCustomFilter = createSelector(
  selectCustomFilterEntities,
  selectSelectedFilterId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);

export const selectCustomFiltersLoading = createSelector(selectCustomFiltersState, (state) => state.loading);

export const selectCustomFiltersError = createSelector(selectCustomFiltersState, (state) => state.error);
