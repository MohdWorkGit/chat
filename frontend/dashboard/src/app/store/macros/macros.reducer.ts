import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Macro } from '@core/models/macro.model';
import { ApiError } from '@core/models/common.model';
import { MacrosActions } from './macros.actions';

export interface MacrosState extends EntityState<Macro> {
  loading: boolean;
  error: ApiError | null;
}

export const macrosAdapter: EntityAdapter<Macro> = createEntityAdapter<Macro>({
  selectId: (macro) => macro.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialMacrosState: MacrosState = macrosAdapter.getInitialState({
  loading: false,
  error: null,
});

export const macrosReducer = createReducer(
  initialMacrosState,

  on(MacrosActions.loadMacros, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(MacrosActions.loadMacrosSuccess, (state, { macros }) =>
    macrosAdapter.setAll(macros, { ...state, loading: false })
  ),

  on(MacrosActions.loadMacrosFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(MacrosActions.createMacroSuccess, (state, { macro }) =>
    macrosAdapter.addOne(macro, state)
  ),

  on(MacrosActions.updateMacroSuccess, (state, { macro }) =>
    macrosAdapter.updateOne({ id: macro.id, changes: macro }, state)
  ),

  on(MacrosActions.deleteMacroSuccess, (state, { id }) =>
    macrosAdapter.removeOne(id, state)
  ),

  on(MacrosActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
