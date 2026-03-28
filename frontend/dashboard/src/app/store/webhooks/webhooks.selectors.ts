import { createFeatureSelector, createSelector } from '@ngrx/store';
import { WebhooksState, webhooksAdapter } from './webhooks.reducer';

export const selectWebhooksState = createFeatureSelector<WebhooksState>('webhooks');

const { selectAll, selectEntities } = webhooksAdapter.getSelectors();

export const selectAllWebhooks = createSelector(selectWebhooksState, selectAll);

export const selectWebhookEntities = createSelector(selectWebhooksState, selectEntities);

export const selectWebhooksLoading = createSelector(selectWebhooksState, (state) => state.loading);

export const selectWebhooksError = createSelector(selectWebhooksState, (state) => state.error);
