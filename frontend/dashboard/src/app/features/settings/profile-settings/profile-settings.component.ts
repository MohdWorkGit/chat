import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '@core/services/auth.service';
import { SettingsService } from '@core/services/settings.service';

@Component({
  selector: 'app-profile-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="max-w-2xl mx-auto px-6 py-6">
      <h3 class="text-lg font-semibold text-gray-900 mb-6">Profile Settings</h3>

      <form [formGroup]="profileForm" (ngSubmit)="saveProfile()" class="space-y-6">
        <!-- Avatar -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Avatar</label>
          <div class="flex items-center gap-4">
            @if (profileForm.get('avatarUrl')?.value) {
              <img [src]="profileForm.get('avatarUrl')?.value" class="h-16 w-16 rounded-full object-cover" alt="Avatar" />
            } @else {
              <div class="h-16 w-16 rounded-full bg-gray-300 flex items-center justify-center text-xl font-medium text-white">
                {{ getInitials(profileForm.get('displayName')?.value || '') }}
              </div>
            }
            <div class="flex-1">
              <input
                formControlName="avatarUrl"
                placeholder="https://example.com/avatar.jpg"
                class="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              <p class="mt-1 text-xs text-gray-400">Enter a URL for your profile picture</p>
            </div>
          </div>
        </div>

        <!-- Display Name -->
        <div>
          <label class="block text-sm font-medium text-gray-700">Display Name</label>
          <input
            formControlName="displayName"
            class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
          @if (profileForm.get('displayName')?.hasError('required') && profileForm.get('displayName')?.touched) {
            <p class="mt-1 text-xs text-red-500">Display name is required</p>
          }
        </div>

        <!-- Email -->
        <div>
          <label class="block text-sm font-medium text-gray-700">Email</label>
          <input
            formControlName="email"
            type="email"
            class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm bg-gray-50 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            readonly
          />
          <p class="mt-1 text-xs text-gray-400">Email cannot be changed</p>
        </div>

        <!-- Availability Status -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Availability Status</label>
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
                <span class="h-2.5 w-2.5 rounded-full" [ngClass]="option.color"></span>
                {{ option.label }}
              </button>
            }
          </div>
        </div>

        <!-- Save Profile -->
        <div class="pt-4 border-t border-gray-200">
          <button
            type="submit"
            [disabled]="profileForm.invalid || profileForm.pristine"
            class="px-6 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Save Profile
          </button>
        </div>
      </form>

      <!-- Password Change Section -->
      <div class="mt-8 pt-6 border-t border-gray-200">
        <h4 class="text-md font-semibold text-gray-900 mb-4">Change Password</h4>
        <form [formGroup]="passwordForm" (ngSubmit)="changePassword()" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700">Current Password</label>
            <input
              formControlName="currentPassword"
              type="password"
              class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">New Password</label>
            <input
              formControlName="newPassword"
              type="password"
              class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            @if (passwordForm.get('newPassword')?.hasError('minlength') && passwordForm.get('newPassword')?.touched) {
              <p class="mt-1 text-xs text-red-500">Password must be at least 8 characters</p>
            }
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Confirm New Password</label>
            <input
              formControlName="confirmPassword"
              type="password"
              class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            @if (passwordMismatch) {
              <p class="mt-1 text-xs text-red-500">Passwords do not match</p>
            }
          </div>
          <div>
            <button
              type="submit"
              [disabled]="passwordForm.invalid || passwordMismatch"
              class="px-6 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Update Password
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class ProfileSettingsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly settingsService = inject(SettingsService);

  availabilityOptions = [
    { value: 'online', label: 'Online', color: 'bg-green-500' },
    { value: 'busy', label: 'Busy', color: 'bg-yellow-500' },
    { value: 'offline', label: 'Offline', color: 'bg-gray-400' },
  ];

  profileForm: FormGroup = this.fb.group({
    displayName: ['', Validators.required],
    email: [''],
    avatarUrl: [''],
    availability: ['online'],
  });

  passwordForm: FormGroup = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  });

  get passwordMismatch(): boolean {
    const { newPassword, confirmPassword } = this.passwordForm.value;
    return confirmPassword?.length > 0 && newPassword !== confirmPassword;
  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe((user) => {
      if (user) {
        this.profileForm.patchValue({
          displayName: user.displayName || user.name,
          email: user.email,
          avatarUrl: user.avatar || '',
          availability: user.availability || 'online',
        });
        this.profileForm.markAsPristine();
      }
    });
  }

  setAvailability(value: string): void {
    this.profileForm.patchValue({ availability: value });
    this.profileForm.markAsDirty();
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    const { displayName, avatarUrl, availability } = this.profileForm.value;
    this.settingsService.updateProfile({
      name: displayName,
      avatarUrl,
      availability,
    }).subscribe(() => {
      this.profileForm.markAsPristine();
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid || this.passwordMismatch) return;
    // Password change would be handled by an auth endpoint
    this.passwordForm.reset();
  }

  getInitials(name: string): string {
    if (!name) return '';
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}
