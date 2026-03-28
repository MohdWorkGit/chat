import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { WebhookService } from '@core/services/webhook.service';
import { WebhooksActions } from './webhooks.actions';
import { ApiError } from '@core/models/common.model';

export const loadWebhooks$ = createEffect(
  (actions$ = inject(Actions), webhookService = inject(WebhookService)) =>
    actions$.pipe(
      ofType(WebhooksActions.loadWebhooks),
      switchMap(() =>
        webhookService.getAll().pipe(
          map((webhooks) => WebhooksActions.loadWebhooksSuccess({ webhooks })),
          catchError((error: ApiError) => of(WebhooksActions.loadWebhooksFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createWebhook$ = createEffect(
  (actions$ = inject(Actions), webhookService = inject(WebhookService)) =>
    actions$.pipe(
      ofType(WebhooksActions.createWebhook),
      exhaustMap(({ data }) =>
        webhookService.create(data).pipe(
          map((webhook) => WebhooksActions.createWebhookSuccess({ webhook })),
          catchError((error: ApiError) => of(WebhooksActions.createWebhookFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateWebhook$ = createEffect(
  (actions$ = inject(Actions), webhookService = inject(WebhookService)) =>
    actions$.pipe(
      ofType(WebhooksActions.updateWebhook),
      exhaustMap(({ id, data }) =>
        webhookService.update(id, data).pipe(
          map((webhook) => WebhooksActions.updateWebhookSuccess({ webhook })),
          catchError((error: ApiError) => of(WebhooksActions.updateWebhookFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteWebhook$ = createEffect(
  (actions$ = inject(Actions), webhookService = inject(WebhookService)) =>
    actions$.pipe(
      ofType(WebhooksActions.deleteWebhook),
      exhaustMap(({ id }) =>
        webhookService.delete(id).pipe(
          map(() => WebhooksActions.deleteWebhookSuccess({ id })),
          catchError((error: ApiError) => of(WebhooksActions.deleteWebhookFailure({ error })))
        )
      )
    ),
  { functional: true }
);
