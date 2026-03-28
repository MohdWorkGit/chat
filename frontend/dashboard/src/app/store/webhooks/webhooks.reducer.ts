import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Webhook } from '@core/models/webhook.model';
import { ApiError } from '@core/models/common.model';
import { WebhooksActions } from './webhooks.actions';

export interface WebhooksState extends EntityState<Webhook> {
  loading: boolean;
  error: ApiError | null;
}

export const webhooksAdapter: EntityAdapter<Webhook> = createEntityAdapter<Webhook>({
  selectId: (webhook) => webhook.id,
});

export const initialWebhooksState: WebhooksState = webhooksAdapter.getInitialState({
  loading: false,
  error: null,
});

export const webhooksReducer = createReducer(
  initialWebhooksState,

  on(WebhooksActions.loadWebhooks, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(WebhooksActions.loadWebhooksSuccess, (state, { webhooks }) =>
    webhooksAdapter.setAll(webhooks, {
      ...state,
      loading: false,
    })
  ),

  on(WebhooksActions.loadWebhooksFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(WebhooksActions.createWebhookSuccess, (state, { webhook }) =>
    webhooksAdapter.addOne(webhook, state)
  ),

  on(WebhooksActions.updateWebhookSuccess, (state, { webhook }) =>
    webhooksAdapter.updateOne({ id: webhook.id, changes: webhook }, state)
  ),

  on(WebhooksActions.deleteWebhookSuccess, (state, { id }) =>
    webhooksAdapter.removeOne(id, state)
  ),

  on(WebhooksActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
