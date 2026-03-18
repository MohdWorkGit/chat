import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Macro } from '@core/models/macro.model';
import { ApiError } from '@core/models/common.model';

export const MacrosActions = createActionGroup({
  source: 'Macros',
  events: {
    'Load Macros': emptyProps(),
    'Load Macros Success': props<{ macros: Macro[] }>(),
    'Load Macros Failure': props<{ error: ApiError }>(),

    'Create Macro': props<{ data: Partial<Macro> }>(),
    'Create Macro Success': props<{ macro: Macro }>(),
    'Create Macro Failure': props<{ error: ApiError }>(),

    'Update Macro': props<{ id: number; data: Partial<Macro> }>(),
    'Update Macro Success': props<{ macro: Macro }>(),
    'Update Macro Failure': props<{ error: ApiError }>(),

    'Delete Macro': props<{ id: number }>(),
    'Delete Macro Success': props<{ id: number }>(),
    'Delete Macro Failure': props<{ error: ApiError }>(),

    'Clear Error': emptyProps(),
  },
});
