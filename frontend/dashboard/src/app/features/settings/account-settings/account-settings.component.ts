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

      <form [formGroup]="accountForm" (ngSubmit)="save()" class="space-y-6">
        <!-- Account Name -->
        <div>
          <label class="block text-sm font-medium text-gray-700">Account Name</label>
          <input
            formControlName="name"
            class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            placeholder="Your organization name"
          />
          @if (accountForm.get('name')?.invalid && accountForm.get('name')?.touched) {
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
            @for (locale of localeOptions; track locale.value) {
              <option [value]="locale.value">{{ locale.label }}</option>
            }
          </select>
        </div>

        <!-- Domain -->
        <div>
          <label class="block text-sm font-medium text-gray-700">Domain</label>
          <input
            formControlName="domain"
            class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            placeholder="yourdomain.com"
          />
        </div>

        <!-- Auto-Resolve Duration -->
        <div>
          <label class="block text-sm font-medium text-gray-700">Auto-Resolve Duration</label>
          <p class="text-xs text-gray-400 mb-2">Automatically resolve conversations after inactivity (in days). Set to 0 to disable.</p>
          <input
            formControlName="autoResolveDuration"
            type="number"
            min="0"
            class="mt-1 block w-40 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        <!-- Feature Toggles -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-3">Features</label>
          <div class="space-y-3">
            @for (feature of featureToggles; track feature.key) {
              <label class="flex items-center justify-between">
                <div>
                  <span class="text-sm text-gray-700">{{ feature.label }}</span>
                  <p class="text-xs text-gray-400">{{ feature.description }}</p>
                </div>
                <div class="relative">
                  <input
                    type="checkbox"
                    [checked]="feature.enabled"
                    (change)="toggleFeature(feature.key)"
                    class="sr-only"
                    [id]="'feature-' + feature.key"
                  />
                  <label
                    [for]="'feature-' + feature.key"
                    class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                    [class]="feature.enabled ? 'bg-blue-600' : 'bg-gray-300'"
                  >
                    <span
                      class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                      [class]="feature.enabled ? 'translate-x-4.5 ml-0.5' : 'translate-x-0.5'"
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
            type="submit"
            [disabled]="accountForm.invalid || (accountForm.pristine && !featuresChanged)"
            class="px-6 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Save Changes
          </button>
          @if (saved) {
            <span class="ml-3 text-sm text-green-600">Settings saved successfully</span>
          }
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

  accountForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    locale: ['en'],
    domain: [''],
    autoResolveDuration: [0],
  });

  localeOptions = [
    { value: 'en', label: 'English' },
    { value: 'es', label: 'Spanish' },
    { value: 'fr', label: 'French' },
    { value: 'de', label: 'German' },
    { value: 'pt', label: 'Portuguese' },
    { value: 'ja', label: 'Japanese' },
    { value: 'zh', label: 'Chinese' },
  ];

  featureToggles = [
    { key: 'auto_resolve', label: 'Auto-Resolve', description: 'Automatically resolve inactive conversations', enabled: false },
    { key: 'business_hours', label: 'Business Hours', description: 'Enable business hours for response time tracking', enabled: false },
    { key: 'email_collect', label: 'Email Collection', description: 'Prompt visitors for email before starting a conversation', enabled: true },
    { key: 'csat', label: 'Customer Satisfaction', description: 'Send CSAT survey after resolving conversations', enabled: false },
  ];

  featuresChanged = false;
  saved = false;

  ngOnInit(): void {
    this.settingsService.getAccountSettings().subscribe((settings) => {
      this.accountForm.patchValue({
        name: settings.name,
        locale: settings.locale,
        domain: settings.domain,
        autoResolveDuration: settings.autoResolveDuration,
      });
      if (settings.features) {
        this.featureToggles.forEach((toggle) => {
          if (settings.features[toggle.key] !== undefined) {
            toggle.enabled = settings.features[toggle.key];
          }
        });
      }
    });
  }

  toggleFeature(key: string): void {
    const feature = this.featureToggles.find((f) => f.key === key);
    if (feature) {
      feature.enabled = !feature.enabled;
      this.featuresChanged = true;
    }
  }

  save(): void {
    if (this.accountForm.invalid) return;
    const features: Record<string, boolean> = {};
    this.featureToggles.forEach((f) => (features[f.key] = f.enabled));

    const payload: Partial<AccountSettings> = {
      ...this.accountForm.value,
      features,
    };

    this.settingsService.updateAccountSettings(payload).subscribe(() => {
      this.accountForm.markAsPristine();
      this.featuresChanged = false;
      this.saved = true;
      setTimeout(() => (this.saved = false), 3000);
    });
  }
}
