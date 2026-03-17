import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { NotificationService } from '@core/services/notification.service';
import { NotificationsActions } from './notifications.actions';
import { ApiError } from '@core/models/common.model';

export const loadNotifications$ = createEffect(
  (actions$ = inject(Actions), notificationService = inject(NotificationService)) =>
    actions$.pipe(
      ofType(NotificationsActions.loadNotifications),
      switchMap(({ page }) =>
        notificationService.getAll(page).pipe(
          map((result) => NotificationsActions.loadNotificationsSuccess({ result })),
          catchError((error: ApiError) => of(NotificationsActions.loadNotificationsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const markAsRead$ = createEffect(
  (actions$ = inject(Actions), notificationService = inject(NotificationService)) =>
    actions$.pipe(
      ofType(NotificationsActions.markAsRead),
      exhaustMap(({ id }) =>
        notificationService.markAsRead(id).pipe(
          map((notification) => NotificationsActions.markAsReadSuccess({ notification })),
          catchError((error: ApiError) => of(NotificationsActions.markAsReadFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const markAllAsRead$ = createEffect(
  (actions$ = inject(Actions), notificationService = inject(NotificationService)) =>
    actions$.pipe(
      ofType(NotificationsActions.markAllAsRead),
      exhaustMap(() =>
        notificationService.markAllAsRead().pipe(
          map(() => NotificationsActions.markAllAsReadSuccess()),
          catchError((error: ApiError) => of(NotificationsActions.markAllAsReadFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadUnreadCount$ = createEffect(
  (actions$ = inject(Actions), notificationService = inject(NotificationService)) =>
    actions$.pipe(
      ofType(NotificationsActions.loadUnreadCount),
      switchMap(() =>
        notificationService.getUnreadCount().pipe(
          map(({ count }) => NotificationsActions.loadUnreadCountSuccess({ count })),
          catchError((error: ApiError) => of(NotificationsActions.loadUnreadCountFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteNotification$ = createEffect(
  (actions$ = inject(Actions), notificationService = inject(NotificationService)) =>
    actions$.pipe(
      ofType(NotificationsActions.deleteNotification),
      exhaustMap(({ id }) =>
        notificationService.delete(id).pipe(
          map(() => NotificationsActions.deleteNotificationSuccess({ id })),
          catchError((error: ApiError) => of(NotificationsActions.deleteNotificationFailure({ error })))
        )
      )
    ),
  { functional: true }
);
