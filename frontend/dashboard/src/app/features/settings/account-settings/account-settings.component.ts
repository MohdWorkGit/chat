import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SettingsService } from '@core/services/settings.service';
import { AccountSettings } from '@core/models/settings.model';

@Component({
  selector: 'app-account-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="max-w-2xl mx-auto px-6 py-6">
      <h3 class="text-lg font-semibold text-gray-900 mb-6">Account Settings</h3>

      <form [formGroup]="accountForm" (ngSubmit)="saveAccount()" class="space-y-6">
        <!-- Account Name -->
        <div>
          <label class="block text-sm font-medium text-gray-700">Account Name</label>
          <input
            formControlName="name"
            class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
          @if (accountForm.get('name')?.hasError('required') && accountForm.get('name')?.touched) {
            <p class="mt-1 text-xs text-red-500">Account name is required</p>
          }
        </div>

        <!-- Locale -->
        <div>
          <label class="block text-sm font-medium text-gray-700">Locale</label>
          <select
            formControlName="locale"
            class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white"
          >
            @for (loc of localeOptions; track loc.value) {
              <option [value]="loc.value">{{ loc.label }}</option>
            }
          </select>
        </div>

        <!-- Timezone -->
        <div>
          <label class="block text-sm font-medium text-gray-700">Timezone</label>
          <select
            formControlName="timezone"
            class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white"
          >
            @for (tz of timezoneOptions; track tz) {
              <option [value]="tz">{{ tz }}</option>
            }
          </select>
        </div>

        <!-- Feature Toggles -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-3">Features</label>
          <div class="space-y-4">
            <!-- Auto-Resolve Duration -->
            <div class="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
              <div>
                <p class="text-sm font-medium text-gray-700">Auto-Resolve Conversations</p>
                <p class="text-xs text-gray-400">Automatically resolve inactive conversations after a set duration</p>
              </div>
              <div class="flex items-center gap-2">
                <input
                  formControlName="autoResolveDuration"
                  type="number"
                  min="0"
                  class="w-20 rounded-lg border border-gray-300 px-3 py-1.5 text-sm text-center focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
                <span class="text-sm text-gray-500">days</span>
              </div>
            </div>

            <!-- Business Hours -->
            <div class="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
              <div>
                <p class="text-sm font-medium text-gray-700">Business Hours</p>
                <p class="text-xs text-gray-400">Enable business hours to set working hours for your team</p>
              </div>
              <div class="relative">
                <input
                  type="checkbox"
                  [checked]="features['business_hours']"
                  (change)="toggleFeature('business_hours')"
                  class="sr-only peer"
                  id="toggle-business-hours"
                />
                <label
                  for="toggle-business-hours"
                  class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                  [ngClass]="features['business_hours'] ? 'bg-blue-600' : 'bg-gray-300'"
                >
                  <span
                    class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                    [ngClass]="features['business_hours'] ? 'translate-x-4 ml-0.5' : 'translate-x-0.5'"
                  ></span>
                </label>
              </div>
            </div>

            <!-- Email Collect -->
            <div class="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
              <div>
                <p class="text-sm font-medium text-gray-700">Email Collect</p>
                <p class="text-xs text-gray-400">Prompt visitors to provide their email before starting a conversation</p>
              </div>
              <div class="relative">
                <input
                  type="checkbox"
                  [checked]="features['email_collect']"
                  (change)="toggleFeature('email_collect')"
                  class="sr-only peer"
                  id="toggle-email-collect"
                />
                <label
                  for="toggle-email-collect"
                  class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                  [ngClass]="features['email_collect'] ? 'bg-blue-600' : 'bg-gray-300'"
                >
                  <span
                    class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                    [ngClass]="features['email_collect'] ? 'translate-x-4 ml-0.5' : 'translate-x-0.5'"
                  ></span>
                </label>
              </div>
            </div>
          </div>
        </div>

        <!-- Save button -->
        <div class="pt-4 border-t border-gray-200">
          <button
            type="submit"
            [disabled]="accountForm.invalid || (accountForm.pristine && !featuresChanged)"
            class="px-6 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Save Settings
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class AccountSettingsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly settingsService = inject(SettingsService);

  features: Record<string, boolean> = {
    business_hours: false,
    email_collect: false,
  };
  featuresChanged = false;

  localeOptions = [
    { value: 'en', label: 'English' },
    { value: 'es', label: 'Spanish' },
    { value: 'fr', label: 'French' },
    { value: 'de', label: 'German' },
    { value: 'pt', label: 'Portuguese' },
    { value: 'ja', label: 'Japanese' },
    { value: 'zh', label: 'Chinese' },
  ];

  timezoneOptions = [
    'UTC',
    'America/New_York',
    'America/Chicago',
    'America/Denver',
    'America/Los_Angeles',
    'Europe/London',
    'Europe/Paris',
    'Europe/Berlin',
    'Asia/Tokyo',
    'Asia/Shanghai',
    'Asia/Kolkata',
    'Australia/Sydney',
  ];

  accountForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    locale: ['en'],
    timezone: ['UTC'],
    autoResolveDuration: [14],
  });

  ngOnInit(): void {
    this.settingsService.getAccountSettings().subscribe((settings) => {
      this.accountForm.patchValue({
        name: settings.name,
        locale: settings.locale,
        autoResolveDuration: settings.autoResolveDuration,
      });
      if (settings.features) {
        this.features = { ...this.features, ...settings.features };
      }
      this.accountForm.markAsPristine();
    });
  }

  toggleFeature(key: string): void {
    this.features[key] = !this.features[key];
    this.featuresChanged = true;
  }

  saveAccount(): void {
    if (this.accountForm.invalid) return;
    const { name, locale, autoResolveDuration } = this.accountForm.value;
    this.settingsService.updateAccountSettings({
      name,
      locale,
      autoResolveDuration,
      features: this.features,
    }).subscribe(() => {
      this.accountForm.markAsPristine();
      this.featuresChanged = false;
    });
  }
}
