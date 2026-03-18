import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CannedResponse } from '@core/models/canned-response.model';
import { ApiError } from '@core/models/common.model';

export const CannedResponsesActions = createActionGroup({
  source: 'CannedResponses',
  events: {
    'Load Canned Responses': emptyProps(),
    'Load Canned Responses Success': props<{ cannedResponses: CannedResponse[] }>(),
    'Load Canned Responses Failure': props<{ error: ApiError }>(),

    'Create Canned Response': props<{ data: Partial<CannedResponse> }>(),
    'Create Canned Response Success': props<{ cannedResponse: CannedResponse }>(),
    'Create Canned Response Failure': props<{ error: ApiError }>(),

    'Update Canned Response': props<{ id: number; data: Partial<CannedResponse> }>(),
    'Update Canned Response Success': props<{ cannedResponse: CannedResponse }>(),
    'Update Canned Response Failure': props<{ error: ApiError }>(),

    'Delete Canned Response': props<{ id: number }>(),
    'Delete Canned Response Success': props<{ id: number }>(),
    'Delete Canned Response Failure': props<{ error: ApiError }>(),

    'Clear Error': emptyProps(),
  },
});
