import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '@core/services/auth.service';
import { SettingsService } from '@core/services/settings.service';
import { NotificationPreferences } from '@core/models/settings.model';

interface NotificationToggle {
  key: keyof NotificationPreferences;
  label: string;
  description: string;
  enabled: boolean;
  category: 'channel' | 'event';
}

@Component({
  selector: 'app-notification-preferences',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="max-w-2xl mx-auto px-6 py-6">
      <h3 class="text-lg font-semibold text-gray-900 mb-2">Notification Preferences</h3>
      <p class="text-sm text-gray-500 mb-6">Choose how and when you want to be notified about activity in your account.</p>

      <!-- Notification Channels -->
      <div class="mb-8">
        <h4 class="text-sm font-semibold text-gray-700 uppercase tracking-wider mb-4">Notification Channels</h4>
        <div class="space-y-3">
          @for (toggle of channelToggles; track toggle.key) {
            <div class="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
              <div>
                <p class="text-sm font-medium text-gray-700">{{ toggle.label }}</p>
                <p class="text-xs text-gray-400">{{ toggle.description }}</p>
              </div>
              <div class="relative">
                <input
                  type="checkbox"
                  [checked]="toggle.enabled"
                  (change)="togglePreference(toggle)"
                  class="sr-only peer"
                  [id]="'notif-' + toggle.key"
                />
                <label
                  [for]="'notif-' + toggle.key"
                  class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                  [ngClass]="toggle.enabled ? 'bg-blue-600' : 'bg-gray-300'"
                >
                  <span
                    class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                    [ngClass]="toggle.enabled ? 'translate-x-4 ml-0.5' : 'translate-x-0.5'"
                  ></span>
                </label>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Event Toggles -->
      <div class="mb-8">
        <h4 class="text-sm font-semibold text-gray-700 uppercase tracking-wider mb-4">Notify Me About</h4>
        <div class="space-y-3">
          @for (toggle of eventToggles; track toggle.key) {
            <div class="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
              <div>
                <p class="text-sm font-medium text-gray-700">{{ toggle.label }}</p>
                <p class="text-xs text-gray-400">{{ toggle.description }}</p>
              </div>
              <div class="relative">
                <input
                  type="checkbox"
                  [checked]="toggle.enabled"
                  (change)="togglePreference(toggle)"
                  class="sr-only peer"
                  [id]="'notif-' + toggle.key"
                />
                <label
                  [for]="'notif-' + toggle.key"
                  class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                  [ngClass]="toggle.enabled ? 'bg-blue-600' : 'bg-gray-300'"
                >
                  <span
                    class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                    [ngClass]="toggle.enabled ? 'translate-x-4 ml-0.5' : 'translate-x-0.5'"
                  ></span>
                </label>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Save button -->
      <div class="pt-4 border-t border-gray-200">
        <button
          (click)="savePreferences()"
          [disabled]="!hasChanges"
          class="px-6 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          Save Preferences
        </button>
        @if (saved) {
          <span class="ml-3 text-sm text-green-600">Preferences saved successfully</span>
        }
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class NotificationPreferencesComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly settingsService = inject(SettingsService);

  hasChanges = false;
  saved = false;

  channelToggles: NotificationToggle[] = [
    {
      key: 'emailNotifications',
      label: 'Email Notifications',
      description: 'Receive notifications via email',
      enabled: true,
      category: 'channel',
    },
    {
      key: 'pushNotifications',
      label: 'Push Notifications',
      description: 'Receive browser push notifications',
      enabled: true,
      category: 'channel',
    },
  ];

  eventToggles: NotificationToggle[] = [
    {
      key: 'mentionNotifications',
      label: 'Mentions',
      description: 'Get notified when someone mentions you in a conversation',
      enabled: true,
      category: 'event',
    },
    {
      key: 'assignmentNotifications',
      label: 'Assignments',
      description: 'Get notified when a conversation is assigned to you',
      enabled: true,
      category: 'event',
    },
  ];

  ngOnInit(): void {
    this.authService.currentUser$.subscribe((user) => {
      if (user) {
        // Preferences would typically come from a dedicated endpoint
      }
    });
  }

  togglePreference(toggle: NotificationToggle): void {
    toggle.enabled = !toggle.enabled;
    this.hasChanges = true;
    this.saved = false;
  }

  savePreferences(): void {
    const prefs: NotificationPreferences = {
      emailNotifications: this.getToggleValue('emailNotifications'),
      pushNotifications: this.getToggleValue('pushNotifications'),
      mentionNotifications: this.getToggleValue('mentionNotifications'),
      assignmentNotifications: this.getToggleValue('assignmentNotifications'),
    };

    this.settingsService.updateNotificationPreferences(prefs).subscribe(() => {
      this.hasChanges = false;
      this.saved = true;
    });
  }

  private getToggleValue(key: keyof NotificationPreferences): boolean {
    const allToggles = [...this.channelToggles, ...this.eventToggles];
    return allToggles.find((t) => t.key === key)?.enabled ?? false;
  }
}
