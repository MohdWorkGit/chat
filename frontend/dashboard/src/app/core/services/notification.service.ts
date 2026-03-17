import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Notification, NotificationSetting } from '@core/models/notification.model';
import { PaginatedResult } from '@core/models/common.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly api = inject(ApiService);
  private readonly basePath = '/notifications';

  getAll(page = 1): Observable<PaginatedResult<Notification>> {
    return this.api.get<PaginatedResult<Notification>>(this.basePath, { page });
  }

  markAsRead(id: number): Observable<Notification> {
    return this.api.patch<Notification>(`${this.basePath}/${id}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.api.post<void>(`${this.basePath}/read-all`);
  }

  snooze(id: number, snoozedUntil: string): Observable<Notification> {
    return this.api.patch<Notification>(`${this.basePath}/${id}/snooze`, { snoozedUntil });
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.api.get<{ count: number }>(`${this.basePath}/unread-count`);
  }

  getSettings(): Observable<NotificationSetting> {
    return this.api.get<NotificationSetting>(`${this.basePath}/settings`);
  }

  updateSettings(settings: Partial<NotificationSetting>): Observable<NotificationSetting> {
    return this.api.patch<NotificationSetting>(`${this.basePath}/settings`, settings);
  }
}
