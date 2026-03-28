import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CustomFilter } from '@core/models/custom-filter.model';
import { ApiError } from '@core/models/common.model';

export const CustomFiltersActions = createActionGroup({
  source: 'Custom Filters',
  events: {
    'Load Custom Filters': emptyProps(),
    'Load Custom Filters Success': props<{ filters: CustomFilter[] }>(),
    'Load Custom Filters Failure': props<{ error: ApiError }>(),

    'Create Custom Filter': props<{ data: Partial<CustomFilter> }>(),
    'Create Custom Filter Success': props<{ filter: CustomFilter }>(),
    'Create Custom Filter Failure': props<{ error: ApiError }>(),

    'Update Custom Filter': props<{ id: number; data: Partial<CustomFilter> }>(),
    'Update Custom Filter Success': props<{ filter: CustomFilter }>(),
    'Update Custom Filter Failure': props<{ error: ApiError }>(),

    'Delete Custom Filter': props<{ id: number }>(),
    'Delete Custom Filter Success': props<{ id: number }>(),
    'Delete Custom Filter Failure': props<{ error: ApiError }>(),

    'Select Custom Filter': props<{ id: number | null }>(),

    'Clear Error': emptyProps(),
  },
});
