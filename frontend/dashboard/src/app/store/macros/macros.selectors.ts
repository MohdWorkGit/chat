import { createFeatureSelector, createSelector } from '@ngrx/store';
import { MacrosState, macrosAdapter } from './macros.reducer';

export const selectMacrosState = createFeatureSelector<MacrosState>('macros');

const { selectAll, selectEntities } = macrosAdapter.getSelectors();

export const selectAllMacros = createSelector(selectMacrosState, selectAll);
export const selectMacroEntities = createSelector(selectMacrosState, selectEntities);
export const selectMacrosLoading = createSelector(selectMacrosState, (state) => state.loading);
export const selectMacrosError = createSelector(selectMacrosState, (state) => state.error);
