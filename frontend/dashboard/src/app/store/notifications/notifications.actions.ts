import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Notification } from '@core/models/notification.model';
import { ApiError, PaginatedResult } from '@core/models/common.model';

export const NotificationsActions = createActionGroup({
  source: 'Notifications',
  events: {
    'Load Notifications': props<{ page?: number }>(),
    'Load Notifications Success': props<{ result: PaginatedResult<Notification> }>(),
    'Load Notifications Failure': props<{ error: ApiError }>(),

    'Mark As Read': props<{ id: number }>(),
    'Mark As Read Success': props<{ notification: Notification }>(),
    'Mark As Read Failure': props<{ error: ApiError }>(),

    'Mark All As Read': emptyProps(),
    'Mark All As Read Success': emptyProps(),
    'Mark All As Read Failure': props<{ error: ApiError }>(),

    'Load Unread Count': emptyProps(),
    'Load Unread Count Success': props<{ count: number }>(),
    'Load Unread Count Failure': props<{ error: ApiError }>(),

    'Notification Received': props<{ notification: Notification }>(),

    'Delete Notification': props<{ id: number }>(),
    'Delete Notification Success': props<{ id: number }>(),
    'Delete Notification Failure': props<{ error: ApiError }>(),
  },
});
