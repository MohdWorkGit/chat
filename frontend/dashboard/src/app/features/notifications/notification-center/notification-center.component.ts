import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { NotificationsActions } from '@store/notifications/notifications.actions';
import {
  selectAllNotifications,
  selectNotificationsLoading,
  selectUnreadCount,
} from '@store/notifications/notifications.selectors';
import { Notification } from '@core/models/notification.model';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center gap-3">
          <h2 class="text-lg font-semibold text-gray-900">Notifications</h2>
          @if ((unreadCount$ | async); as unreadCount) {
            @if (unreadCount > 0) {
              <span class="inline-flex items-center justify-center h-5 min-w-[20px] px-1.5 rounded-full bg-red-500 text-xs font-medium text-white">
                {{ unreadCount }}
              </span>
            }
          }
        </div>
        <button
          (click)="markAllAsRead()"
          class="px-4 py-2 text-sm font-medium text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 transition-colors"
        >
          Mark all as read
        </button>
      </div>

      <!-- Loading State -->
      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((notifications$ | async); as notifications) {
          @if (notifications.length === 0) {
            <div class="text-center py-12">
              <svg class="mx-auto h-12 w-12 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M14.857 17.082a23.848 23.848 0 0 0 5.454-1.31A8.967 8.967 0 0 1 18 9.75V9A6 6 0 0 0 6 9v.75a8.967 8.967 0 0 1-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 0 1-5.714 0m5.714 0a3 3 0 1 1-5.714 0" />
              </svg>
              <p class="mt-2 text-sm text-gray-500">No notifications yet.</p>
            </div>
          } @else {
            <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <ul class="divide-y divide-gray-200">
                @for (notification of notifications; track notification.id) {
                  <li
                    class="px-6 py-4 hover:bg-gray-50 transition-colors cursor-pointer"
                    [class]="notification.readAt ? 'bg-white' : 'bg-blue-50'"
                    (click)="markAsRead(notification)"
                  >
                    <div class="flex items-start gap-3">
                      <!-- Type Icon -->
                      <div
                        class="flex-shrink-0 h-9 w-9 rounded-full flex items-center justify-center"
                        [class]="getIconBgClass(notification.notificationType)"
                      >
                        <span [innerHTML]="getTypeIcon(notification.notificationType)"></span>
                      </div>

                      <!-- Content -->
                      <div class="flex-1 min-w-0">
                        <p class="text-sm text-gray-900" [class.font-medium]="!notification.readAt">
                          {{ getNotificationMessage(notification) }}
                        </p>
                        <p class="mt-1 text-xs text-gray-500">
                          {{ formatTimestamp(notification.createdAt) }}
                        </p>
                      </div>

                      <!-- Unread Indicator -->
                      @if (!notification.readAt) {
                        <span class="flex-shrink-0 h-2 w-2 rounded-full bg-blue-600 mt-2"></span>
                      }
                    </div>
                  </li>
                }
              </ul>
            </div>
          }
        }
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class NotificationCenterComponent implements OnInit {
  private store = inject(Store);

  notifications$ = this.store.select(selectAllNotifications);
  loading$ = this.store.select(selectNotificationsLoading);
  unreadCount$ = this.store.select(selectUnreadCount);

  ngOnInit(): void {
    this.store.dispatch(NotificationsActions.loadNotifications({}));
    this.store.dispatch(NotificationsActions.loadUnreadCount());
  }

  markAsRead(notification: Notification): void {
    if (!notification.readAt) {
      this.store.dispatch(NotificationsActions.markAsRead({ id: notification.id }));
    }
  }

  markAllAsRead(): void {
    this.store.dispatch(NotificationsActions.markAllAsRead());
  }

  getNotificationMessage(notification: Notification): string {
    const typeLabels: Record<string, string> = {
      conversation_creation: 'A new conversation has been created',
      conversation_assignment: 'A conversation has been assigned to you',
      assigned_conversation_new_message: 'New message in your assigned conversation',
      participating_conversation_new_message: 'New message in a conversation you are participating in',
      mention: 'You were mentioned in a conversation',
    };
    return typeLabels[notification.notificationType] || 'You have a new notification';
  }

  getTypeIcon(type: string): string {
    const icons: Record<string, string> = {
      conversation_creation:
        '<svg class="h-4 w-4 text-green-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" /></svg>',
      conversation_assignment:
        '<svg class="h-4 w-4 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" /></svg>',
      assigned_conversation_new_message:
        '<svg class="h-4 w-4 text-purple-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M8.625 12a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm0 0H8.25m4.125 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm0 0H12m4.125 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm0 0h-.375M21 12c0 4.556-4.03 8.25-9 8.25a9.764 9.764 0 0 1-2.555-.337A5.972 5.972 0 0 1 5.41 20.97a5.969 5.969 0 0 1-.474-.065 4.48 4.48 0 0 0 .978-2.025c.09-.457-.133-.901-.467-1.226C3.93 16.178 3 14.189 3 12c0-4.556 4.03-8.25 9-8.25s9 3.694 9 8.25Z" /></svg>',
      participating_conversation_new_message:
        '<svg class="h-4 w-4 text-indigo-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M20.25 8.511c.884.284 1.5 1.128 1.5 2.097v4.286c0 1.136-.847 2.1-1.98 2.193-.34.027-.68.052-1.02.072v3.091l-3-3c-1.354 0-2.694-.055-4.02-.163a2.115 2.115 0 0 1-.825-.242m9.345-8.334a2.126 2.126 0 0 0-.476-.095 48.64 48.64 0 0 0-8.048 0c-1.131.094-1.976 1.057-1.976 2.192v4.286c0 .837.46 1.58 1.155 1.951m9.345-8.334V6.637c0-1.621-1.152-3.026-2.76-3.235A48.455 48.455 0 0 0 11.25 3c-2.115 0-4.198.137-6.24.402-1.608.209-2.76 1.614-2.76 3.235v6.226c0 1.621 1.152 3.026 2.76 3.235.577.075 1.157.14 1.74.194V21l4.155-4.155" /></svg>',
      mention:
        '<svg class="h-4 w-4 text-orange-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M16.5 12a4.5 4.5 0 1 1-9 0 4.5 4.5 0 0 1 9 0Zm0 0c0 1.657 1.007 3 2.25 3S21 13.657 21 12a9 9 0 1 0-2.636 6.364M16.5 12V8.25" /></svg>',
    };
    return icons[type] || icons['conversation_creation'];
  }

  getIconBgClass(type: string): string {
    const classes: Record<string, string> = {
      conversation_creation: 'bg-green-100',
      conversation_assignment: 'bg-blue-100',
      assigned_conversation_new_message: 'bg-purple-100',
      participating_conversation_new_message: 'bg-indigo-100',
      mention: 'bg-orange-100',
    };
    return classes[type] || 'bg-gray-100';
  }

  formatTimestamp(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  }
}
