import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Notification } from '@core/models/notification.model';
import { ApiError } from '@core/models/common.model';
import { NotificationsActions } from './notifications.actions';

export interface NotificationsState extends EntityState<Notification> {
  unreadCount: number;
  totalCount: number;
  currentPage: number;
  totalPages: number;
  loading: boolean;
  error: ApiError | null;
}

export const notificationsAdapter: EntityAdapter<Notification> = createEntityAdapter<Notification>({
  selectId: (notification) => notification.id,
  sortComparer: (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
});

export const initialNotificationsState: NotificationsState = notificationsAdapter.getInitialState({
  unreadCount: 0,
  totalCount: 0,
  currentPage: 1,
  totalPages: 0,
  loading: false,
  error: null,
});

export const notificationsReducer = createReducer(
  initialNotificationsState,

  on(NotificationsActions.loadNotifications, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(NotificationsActions.loadNotificationsSuccess, (state, { result }) =>
    notificationsAdapter.setAll(result.data, {
      ...state,
      totalCount: result.meta.totalCount,
      currentPage: result.meta.currentPage,
      totalPages: result.meta.totalPages,
      loading: false,
    })
  ),

  on(NotificationsActions.loadNotificationsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(NotificationsActions.markAsReadSuccess, (state, { notification }) =>
    notificationsAdapter.updateOne(
      { id: notification.id, changes: notification },
      { ...state, unreadCount: Math.max(0, state.unreadCount - 1) }
    )
  ),

  on(NotificationsActions.markAllAsReadSuccess, (state) => ({
    ...state,
    unreadCount: 0,
  })),

  on(NotificationsActions.loadUnreadCountSuccess, (state, { count }) => ({
    ...state,
    unreadCount: count,
  })),

  on(NotificationsActions.notificationReceived, (state, { notification }) =>
    notificationsAdapter.addOne(notification, {
      ...state,
      unreadCount: state.unreadCount + 1,
      totalCount: state.totalCount + 1,
    })
  ),

  on(NotificationsActions.deleteNotificationSuccess, (state, { id }) =>
    notificationsAdapter.removeOne(id, {
      ...state,
      totalCount: state.totalCount - 1,
    })
  )
);
