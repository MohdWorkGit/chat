import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { AutomationRule } from '@core/models/automation.model';
import { ApiError } from '@core/models/common.model';
import { AutomationsActions } from './automations.actions';

export interface AutomationsState extends EntityState<AutomationRule> {
  selectedAutomationId: number | null;
  loading: boolean;
  error: ApiError | null;
}

export const automationsAdapter: EntityAdapter<AutomationRule> = createEntityAdapter<AutomationRule>({
  selectId: (rule) => rule.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialAutomationsState: AutomationsState = automationsAdapter.getInitialState({
  selectedAutomationId: null,
  loading: false,
  error: null,
});

export const automationsReducer = createReducer(
  initialAutomationsState,

  on(AutomationsActions.loadAutomations, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(AutomationsActions.loadAutomationsSuccess, (state, { automations }) =>
    automationsAdapter.setAll(automations, { ...state, loading: false })
  ),

  on(AutomationsActions.loadAutomationsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(AutomationsActions.createAutomationSuccess, (state, { automation }) =>
    automationsAdapter.addOne(automation, state)
  ),

  on(AutomationsActions.updateAutomationSuccess, (state, { automation }) =>
    automationsAdapter.updateOne({ id: automation.id, changes: automation }, state)
  ),

  on(AutomationsActions.deleteAutomationSuccess, (state, { id }) =>
    automationsAdapter.removeOne(id, state)
  ),

  on(AutomationsActions.selectAutomation, (state, { id }) => ({
    ...state,
    selectedAutomationId: id,
  })),

  on(AutomationsActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
