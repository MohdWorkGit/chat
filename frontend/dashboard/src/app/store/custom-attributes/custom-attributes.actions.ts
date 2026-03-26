import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CustomAttribute } from '@core/models/custom-attribute.model';
import { ApiError } from '@core/models/common.model';

export const CustomAttributesActions = createActionGroup({
  source: 'CustomAttributes',
  events: {
    'Load Custom Attributes': emptyProps(),
    'Load Custom Attributes Success': props<{ customAttributes: CustomAttribute[] }>(),
    'Load Custom Attributes Failure': props<{ error: ApiError }>(),

    'Create Custom Attribute': props<{ data: Partial<CustomAttribute> }>(),
    'Create Custom Attribute Success': props<{ customAttribute: CustomAttribute }>(),
    'Create Custom Attribute Failure': props<{ error: ApiError }>(),

    'Update Custom Attribute': props<{ id: number; data: Partial<CustomAttribute> }>(),
    'Update Custom Attribute Success': props<{ customAttribute: CustomAttribute }>(),
    'Update Custom Attribute Failure': props<{ error: ApiError }>(),

    'Delete Custom Attribute': props<{ id: number }>(),
    'Delete Custom Attribute Success': props<{ id: number }>(),
    'Delete Custom Attribute Failure': props<{ error: ApiError }>(),

    'Clear Error': emptyProps(),
  },
});
