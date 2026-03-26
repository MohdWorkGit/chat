import { createFeatureSelector, createSelector } from '@ngrx/store';
import { CustomAttributesState, customAttributesAdapter } from './custom-attributes.reducer';

export const selectCustomAttributesState = createFeatureSelector<CustomAttributesState>('customAttributes');

const { selectAll, selectEntities } = customAttributesAdapter.getSelectors();

export const selectAllCustomAttributes = createSelector(selectCustomAttributesState, selectAll);
export const selectCustomAttributeEntities = createSelector(selectCustomAttributesState, selectEntities);
export const selectCustomAttributesLoading = createSelector(selectCustomAttributesState, (state) => state.loading);
export const selectCustomAttributesError = createSelector(selectCustomAttributesState, (state) => state.error);
