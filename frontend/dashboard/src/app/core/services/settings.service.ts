import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { AccountSettings, UserProfile, NotificationPreferences } from '@core/models/settings.model';

@Injectable({
  providedIn: 'root',
})
export class SettingsService {
  private readonly api = inject(ApiService);

  getAccountSettings(): Observable<AccountSettings> {
    return this.api.get('/accounts/current');
  }

  updateAccountSettings(data: Partial<AccountSettings>): Observable<AccountSettings> {
    return this.api.put('/accounts/current', data);
  }

  getProfile(): Observable<UserProfile> {
    return this.api.get('/profile');
  }

  updateProfile(data: Partial<UserProfile>): Observable<UserProfile> {
    return this.api.put('/profile', data);
  }

  updateNotificationPreferences(prefs: NotificationPreferences): Observable<NotificationPreferences> {
    return this.api.put('/profile/notifications', prefs);
  }
}
