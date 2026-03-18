import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { AutomationRule } from '@core/models/automation.model';
import { ApiError } from '@core/models/common.model';

export const AutomationsActions = createActionGroup({
  source: 'Automations',
  events: {
    'Load Automations': emptyProps(),
    'Load Automations Success': props<{ automations: AutomationRule[] }>(),
    'Load Automations Failure': props<{ error: ApiError }>(),

    'Create Automation': props<{ data: Partial<AutomationRule> }>(),
    'Create Automation Success': props<{ automation: AutomationRule }>(),
    'Create Automation Failure': props<{ error: ApiError }>(),

    'Update Automation': props<{ id: number; data: Partial<AutomationRule> }>(),
    'Update Automation Success': props<{ automation: AutomationRule }>(),
    'Update Automation Failure': props<{ error: ApiError }>(),

    'Delete Automation': props<{ id: number }>(),
    'Delete Automation Success': props<{ id: number }>(),
    'Delete Automation Failure': props<{ error: ApiError }>(),

    'Select Automation': props<{ id: number | null }>(),
    'Clear Error': emptyProps(),
  },
});
