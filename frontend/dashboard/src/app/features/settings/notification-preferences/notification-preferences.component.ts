import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SettingsService } from '@core/services/settings.service';
import { AuthService } from '@core/services/auth.service';
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
      <p class="text-sm text-gray-500 mb-6">Choose how and when you want to be notified.</p>

      <!-- Notification Channels -->
      <div class="mb-8">
        <h4 class="text-sm font-medium text-gray-900 mb-4 uppercase tracking-wide">Notification Channels</h4>
        <div class="space-y-4 bg-white rounded-lg border border-gray-200 p-4">
          @for (toggle of channelToggles; track toggle.key) {
            <label class="flex items-center justify-between">
              <div>
                <span class="text-sm font-medium text-gray-700">{{ toggle.label }}</span>
                <p class="text-xs text-gray-400">{{ toggle.description }}</p>
              </div>
              <div class="relative">
                <input
                  type="checkbox"
                  [checked]="toggle.enabled"
                  (change)="togglePreference(toggle)"
                  class="sr-only"
                  [id]="'notif-' + toggle.key"
                />
                <label
                  [for]="'notif-' + toggle.key"
                  class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                  [class]="toggle.enabled ? 'bg-blue-600' : 'bg-gray-300'"
                >
                  <span
                    class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                    [class]="toggle.enabled ? 'translate-x-4.5 ml-0.5' : 'translate-x-0.5'"
                  ></span>
                </label>
              </div>
            </label>
          }
        </div>
      </div>

      <!-- Per-Event Toggles -->
      <div class="mb-8">
        <h4 class="text-sm font-medium text-gray-900 mb-4 uppercase tracking-wide">Event Notifications</h4>
        <div class="space-y-4 bg-white rounded-lg border border-gray-200 p-4">
          @for (toggle of eventToggles; track toggle.key) {
            <label class="flex items-center justify-between">
              <div>
                <span class="text-sm font-medium text-gray-700">{{ toggle.label }}</span>
                <p class="text-xs text-gray-400">{{ toggle.description }}</p>
              </div>
              <div class="relative">
                <input
                  type="checkbox"
                  [checked]="toggle.enabled"
                  (change)="togglePreference(toggle)"
                  class="sr-only"
                  [id]="'notif-' + toggle.key"
                />
                <label
                  [for]="'notif-' + toggle.key"
                  class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                  [class]="toggle.enabled ? 'bg-blue-600' : 'bg-gray-300'"
                >
                  <span
                    class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                    [class]="toggle.enabled ? 'translate-x-4.5 ml-0.5' : 'translate-x-0.5'"
                  ></span>
                </label>
              </div>
            </label>
          }
        </div>
      </div>

      <!-- Save -->
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
  private readonly settingsService = inject(SettingsService);
  private readonly authService = inject(AuthService);

  channelToggles: NotificationToggle[] = [
    { key: 'emailNotifications', label: 'Email Notifications', description: 'Receive notification emails for conversations and updates', enabled: true, category: 'channel' },
    { key: 'pushNotifications', label: 'Push Notifications', description: 'Receive browser push notifications in real-time', enabled: true, category: 'channel' },
  ];

  eventToggles: NotificationToggle[] = [
    { key: 'emailNotifications', label: 'New Conversation', description: 'When a new conversation is created in your inbox', enabled: true, category: 'event' },
    { key: 'pushNotifications', label: 'New Message', description: 'When a new message arrives in an assigned conversation', enabled: true, category: 'event' },
    { key: 'mentionNotifications', label: 'Mentions', description: 'When someone mentions you in a conversation or note', enabled: true, category: 'event' },
    { key: 'assignmentNotifications', label: 'Assignment', description: 'When a conversation is assigned to you', enabled: true, category: 'event' },
  ];

  hasChanges = false;
  saved = false;

  ngOnInit(): void {
    this.authService.currentUser$.subscribe((user) => {
      // Load preferences if user is available
      if (user) {
        this.loadPreferences();
      }
    });
  }

  private loadPreferences(): void {
    // Preferences would be loaded from the settings service in a real app
    // For now they use the defaults set above
  }

  togglePreference(toggle: NotificationToggle): void {
    toggle.enabled = !toggle.enabled;
    this.hasChanges = true;
  }

  savePreferences(): void {
    const prefs: NotificationPreferences = {
      emailNotifications: this.channelToggles.find((t) => t.key === 'emailNotifications')?.enabled ?? true,
      pushNotifications: this.channelToggles.find((t) => t.key === 'pushNotifications')?.enabled ?? true,
      mentionNotifications: this.eventToggles.find((t) => t.key === 'mentionNotifications')?.enabled ?? true,
      assignmentNotifications: this.eventToggles.find((t) => t.key === 'assignmentNotifications')?.enabled ?? true,
    };

    this.settingsService.updateNotificationPreferences(prefs).subscribe(() => {
      this.hasChanges = false;
      this.saved = true;
      setTimeout(() => (this.saved = false), 3000);
    });
  }
}
