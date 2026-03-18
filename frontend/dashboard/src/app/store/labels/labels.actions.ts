import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Label } from '@core/models/label.model';
import { ApiError } from '@core/models/common.model';

export const LabelsActions = createActionGroup({
  source: 'Labels',
  events: {
    'Load Labels': emptyProps(),
    'Load Labels Success': props<{ labels: Label[] }>(),
    'Load Labels Failure': props<{ error: ApiError }>(),

    'Load Label': props<{ id: number }>(),
    'Load Label Success': props<{ label: Label }>(),
    'Load Label Failure': props<{ error: ApiError }>(),

    'Create Label': props<{ data: Partial<Label> }>(),
    'Create Label Success': props<{ label: Label }>(),
    'Create Label Failure': props<{ error: ApiError }>(),

    'Update Label': props<{ id: number; data: Partial<Label> }>(),
    'Update Label Success': props<{ label: Label }>(),
    'Update Label Failure': props<{ error: ApiError }>(),

    'Delete Label': props<{ id: number }>(),
    'Delete Label Success': props<{ id: number }>(),
    'Delete Label Failure': props<{ error: ApiError }>(),

    'Select Label': props<{ id: number | null }>(),

    'Clear Error': emptyProps(),
  },
});
