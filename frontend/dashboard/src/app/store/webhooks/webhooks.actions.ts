import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Webhook } from '@core/models/webhook.model';
import { ApiError } from '@core/models/common.model';

export const WebhooksActions = createActionGroup({
  source: 'Webhooks',
  events: {
    'Load Webhooks': emptyProps(),
    'Load Webhooks Success': props<{ webhooks: Webhook[] }>(),
    'Load Webhooks Failure': props<{ error: ApiError }>(),

    'Create Webhook': props<{ data: Partial<Webhook> }>(),
    'Create Webhook Success': props<{ webhook: Webhook }>(),
    'Create Webhook Failure': props<{ error: ApiError }>(),

    'Update Webhook': props<{ id: number; data: Partial<Webhook> }>(),
    'Update Webhook Success': props<{ webhook: Webhook }>(),
    'Update Webhook Failure': props<{ error: ApiError }>(),

    'Delete Webhook': props<{ id: number }>(),
    'Delete Webhook Success': props<{ id: number }>(),
    'Delete Webhook Failure': props<{ error: ApiError }>(),

    'Clear Error': emptyProps(),
  },
});
