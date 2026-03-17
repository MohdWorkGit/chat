import { createFeatureSelector, createSelector } from '@ngrx/store';
import { NotificationsState, notificationsAdapter } from './notifications.reducer';

export const selectNotificationsState = createFeatureSelector<NotificationsState>('notifications');

const { selectAll, selectTotal } = notificationsAdapter.getSelectors();

export const selectAllNotifications = createSelector(selectNotificationsState, selectAll);

export const selectNotificationsTotal = createSelector(selectNotificationsState, selectTotal);

export const selectUnreadCount = createSelector(selectNotificationsState, (state) => state.unreadCount);

export const selectNotificationsLoading = createSelector(selectNotificationsState, (state) => state.loading);

export const selectNotificationsError = createSelector(selectNotificationsState, (state) => state.error);

export const selectNotificationsPagination = createSelector(selectNotificationsState, (state) => ({
  currentPage: state.currentPage,
  totalPages: state.totalPages,
  totalCount: state.totalCount,
}));

export const selectUnreadNotifications = createSelector(selectAllNotifications, (notifications) =>
  notifications.filter((n) => !n.readAt)
);
