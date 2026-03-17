import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { AuthActions } from '@app/store/auth/auth.actions';
import { selectCurrentUser } from '@app/store/auth/auth.selectors';

@Component({
  selector: 'app-settings-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, ReactiveFormsModule],
  template: `
    <div class="flex h-full bg-gray-50">
      <!-- Sidebar Navigation -->
      <nav class="w-56 bg-white border-r border-gray-200 flex-shrink-0">
        <div class="px-4 py-5 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">Settings</h2>
        </div>
        <ul class="py-2">
          @for (item of navItems; track item.path) {
            <li>
              <a
                [routerLink]="item.path"
                routerLinkActive="bg-blue-50 text-blue-700 border-r-2 border-blue-600"
                class="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50 transition-colors"
              >
                <span [innerHTML]="item.icon"></span>
                {{ item.label }}
              </a>
            </li>
          }
        </ul>
      </nav>

      <!-- Content Area -->
      <div class="flex-1 overflow-y-auto">
        <!-- Profile section (default view) -->
        @if (showProfile) {
          <div class="max-w-2xl mx-auto px-6 py-6">
            <h3 class="text-lg font-semibold text-gray-900 mb-6">Profile Settings</h3>

            @if (user$ | async; as user) {
              <form [formGroup]="profileForm" (ngSubmit)="saveProfile()" class="space-y-6">
                <!-- Avatar -->
                <div class="flex items-center gap-4">
                  @if (user.avatar) {
                    <img [src]="user.avatar" class="h-16 w-16 rounded-full object-cover" />
                  } @else {
                    <div class="h-16 w-16 rounded-full bg-gray-300 flex items-center justify-center text-xl font-medium text-white">
                      {{ getInitials(user.name) }}
                    </div>
                  }
                  <button
                    type="button"
                    class="px-3 py-1.5 text-sm text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                  >
                    Change Avatar
                  </button>
                </div>

                <!-- Name -->
                <div>
                  <label class="block text-sm font-medium text-gray-700">Full Name</label>
                  <input
                    formControlName="name"
                    class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>

                <!-- Email -->
                <div>
                  <label class="block text-sm font-medium text-gray-700">Email</label>
                  <input
                    formControlName="email"
                    type="email"
                    class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 bg-gray-50"
                    readonly
                  />
                  <p class="mt-1 text-xs text-gray-400">Email cannot be changed</p>
                </div>

                <!-- Availability -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Availability</label>
                  <div class="flex gap-3">
                    @for (option of availabilityOptions; track option.value) {
                      <button
                        type="button"
                        (click)="setAvailability(option.value)"
                        class="flex items-center gap-2 px-4 py-2 rounded-lg border text-sm transition-colors"
                        [class]="profileForm.get('availability')?.value === option.value
                          ? 'border-blue-500 bg-blue-50 text-blue-700'
                          : 'border-gray-300 text-gray-700 hover:bg-gray-50'"
                      >
                        <span
                          class="h-2.5 w-2.5 rounded-full"
                          [class]="option.color"
                        ></span>
                        {{ option.label }}
                      </button>
                    }
                  </div>
                </div>

                <!-- Notification Preferences -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-3">Notification Preferences</label>
                  <div class="space-y-3">
                    @for (pref of notificationPrefs; track pref.key) {
                      <label class="flex items-center justify-between">
                        <div>
                          <span class="text-sm text-gray-700">{{ pref.label }}</span>
                          <p class="text-xs text-gray-400">{{ pref.description }}</p>
                        </div>
                        <div class="relative">
                          <input
                            type="checkbox"
                            [checked]="pref.enabled"
                            (change)="toggleNotification(pref.key)"
                            class="sr-only peer"
                            [id]="'notif-' + pref.key"
                          />
                          <label
                            [for]="'notif-' + pref.key"
                            class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                            [class]="pref.enabled ? 'bg-blue-600' : 'bg-gray-300'"
                          >
                            <span
                              class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                              [class]="pref.enabled ? 'translate-x-4.5 ml-0.5' : 'translate-x-0.5'"
                            ></span>
                          </label>
                        </div>
                      </label>
                    }
                  </div>
                </div>

                <!-- Save button -->
                <div class="pt-4 border-t border-gray-200">
                  <button
                    type="submit"
                    [disabled]="profileForm.invalid || profileForm.pristine"
                    class="px-6 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    Save Changes
                  </button>
                </div>
              </form>
            }
          </div>
        } @else {
          <router-outlet />
        }
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class SettingsLayoutComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  user$ = this.store.select(selectCurrentUser);

  showProfile = true;

  navItems = [
    {
      label: 'Profile',
      path: '/settings/profile',
      icon: '<svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" /></svg>',
    },
    {
      label: 'Account',
      path: '/settings/account',
      icon: '<svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 0 1 1.37.49l1.296 2.247a1.125 1.125 0 0 1-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 0 1 0 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 0 1-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 0 1-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 0 1-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 0 1-1.369-.49l-1.297-2.247a1.125 1.125 0 0 1 .26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 0 1 0-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 0 1-.26-1.43l1.297-2.247a1.125 1.125 0 0 1 1.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28Z" /><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" /></svg>',
    },
    {
      label: 'Inboxes',
      path: '/settings/inboxes',
      icon: '<svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M2.25 13.5h3.86a2.25 2.25 0 0 1 2.012 1.244l.256.512a2.25 2.25 0 0 0 2.013 1.244h3.218a2.25 2.25 0 0 0 2.013-1.244l.256-.512a2.25 2.25 0 0 1 2.013-1.244h3.859m-19.5.338V18a2.25 2.25 0 0 0 2.25 2.25h15A2.25 2.25 0 0 0 21.75 18v-4.162c0-.224-.034-.447-.1-.661L19.24 5.338a2.25 2.25 0 0 0-2.15-1.588H6.911a2.25 2.25 0 0 0-2.15 1.588L2.35 13.177a2.25 2.25 0 0 0-.1.661Z" /></svg>',
    },
    {
      label: 'Teams',
      path: '/settings/teams',
      icon: '<svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M18 18.72a9.094 9.094 0 0 0 3.741-.479 3 3 0 0 0-4.682-2.72m.94 3.198.001.031c0 .225-.012.447-.037.666A11.944 11.944 0 0 1 12 21c-2.17 0-4.207-.576-5.963-1.584A6.062 6.062 0 0 1 6 18.719m12 0a5.971 5.971 0 0 0-.941-3.197m0 0A5.995 5.995 0 0 0 12 12.75a5.995 5.995 0 0 0-5.058 2.772m0 0a3 3 0 0 0-4.681 2.72 8.986 8.986 0 0 0 3.74.477m.94-3.197a5.971 5.971 0 0 0-.94 3.197M15 6.75a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm6 3a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Zm-13.5 0a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Z" /></svg>',
    },
    {
      label: 'Labels',
      path: '/settings/labels',
      icon: '<svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M9.568 3H5.25A2.25 2.25 0 0 0 3 5.25v4.318c0 .597.237 1.17.659 1.591l9.581 9.581c.699.699 1.78.872 2.607.33a18.095 18.095 0 0 0 5.223-5.223c.542-.827.369-1.908-.33-2.607L11.16 3.66A2.25 2.25 0 0 0 9.568 3Z" /><path stroke-linecap="round" stroke-linejoin="round" d="M6 6h.008v.008H6V6Z" /></svg>',
    },
    {
      label: 'Canned Responses',
      path: '/settings/canned-responses',
      icon: '<svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M7.5 8.25h9m-9 3H12m-9.75 1.51c0 1.6 1.123 2.994 2.707 3.227 1.129.166 2.27.293 3.423.379.35.026.67.21.865.501L12 21l2.755-4.133a1.14 1.14 0 0 1 .865-.501 48.172 48.172 0 0 0 3.423-.379c1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0 0 12 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018Z" /></svg>',
    },
    {
      label: 'Automation',
      path: '/settings/automation',
      icon: '<svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M3.75 13.5l10.5-11.25L12 10.5h8.25L9.75 21.75 12 13.5H3.75Z" /></svg>',
    },
  ];

  availabilityOptions = [
    { value: 'online', label: 'Online', color: 'bg-green-500' },
    { value: 'busy', label: 'Busy', color: 'bg-yellow-500' },
    { value: 'offline', label: 'Offline', color: 'bg-gray-400' },
  ];

  notificationPrefs = [
    {
      key: 'email_notifications',
      label: 'Email Notifications',
      description: 'Receive email alerts for new conversations',
      enabled: true,
    },
    {
      key: 'push_notifications',
      label: 'Push Notifications',
      description: 'Browser push notifications for new messages',
      enabled: true,
    },
    {
      key: 'mention_notifications',
      label: 'Mention Alerts',
      description: 'Get notified when someone mentions you',
      enabled: true,
    },
    {
      key: 'sound_notifications',
      label: 'Sound Alerts',
      description: 'Play a sound for incoming messages',
      enabled: false,
    },
  ];

  profileForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    email: [''],
    availability: ['online'],
  });

  ngOnInit(): void {
    this.user$.subscribe((user) => {
      if (user) {
        this.profileForm.patchValue({
          name: user.name,
          email: user.email,
          availability: user.availability || 'online',
        });
      }
    });

    // Determine if we should show profile based on current route
    const url = this.router.url;
    this.showProfile = url === '/settings' || url === '/settings/profile';
    this.router.events.subscribe(() => {
      const currentUrl = this.router.url;
      this.showProfile = currentUrl === '/settings' || currentUrl === '/settings/profile';
    });
  }

  setAvailability(value: string): void {
    this.profileForm.patchValue({ availability: value });
    this.profileForm.markAsDirty();
    this.store.dispatch(AuthActions.updateAvailability({ availability: value }));
  }

  toggleNotification(key: string): void {
    const pref = this.notificationPrefs.find((p) => p.key === key);
    if (pref) {
      pref.enabled = !pref.enabled;
    }
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    // In a real app, dispatch an action to update user profile
    const { name, availability } = this.profileForm.value;
    // Could dispatch a profile update action here
    this.profileForm.markAsPristine();
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}
