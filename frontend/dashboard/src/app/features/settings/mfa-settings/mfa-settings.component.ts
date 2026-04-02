import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-mfa-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="max-w-2xl mx-auto p-6">
      <h2 class="text-xl font-semibold text-gray-900 mb-6">Two-Factor Authentication</h2>

      @if (!mfaEnabled()) {
        <!-- MFA Not Enabled -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <div class="flex items-start gap-4">
            <div class="flex-shrink-0 w-10 h-10 rounded-full bg-yellow-100 flex items-center justify-center">
              <svg class="w-5 h-5 text-yellow-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m0 0v2m0-2h2m-2 0H10m9.364-7.364A9 9 0 1112 3a9 9 0 017.364 4.636z"/>
              </svg>
            </div>
            <div class="flex-1">
              <h3 class="text-lg font-medium text-gray-900">MFA is not enabled</h3>
              <p class="text-sm text-gray-500 mt-1">Add an extra layer of security to your account by enabling two-factor authentication.</p>
              <button
                (click)="setupMfa()"
                [disabled]="loading()"
                class="mt-4 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-md hover:bg-indigo-700 disabled:opacity-50">
                {{ loading() ? 'Setting up...' : 'Enable Two-Factor Authentication' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Setup Flow -->
        @if (setupData()) {
          <div class="mt-6 bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-lg font-medium text-gray-900 mb-4">Scan QR Code</h3>
            <p class="text-sm text-gray-600 mb-4">Scan the QR code below with your authenticator app (Google Authenticator, Authy, etc.)</p>

            <div class="bg-gray-50 p-4 rounded-md mb-4">
              <p class="text-xs text-gray-500 mb-2">Or enter this key manually:</p>
              <code class="text-sm font-mono bg-gray-100 px-2 py-1 rounded">{{ setupData()!.secretKey }}</code>
            </div>

            <div class="mt-4">
              <label class="block text-sm font-medium text-gray-700 mb-1">Verification Code</label>
              <div class="flex gap-3">
                <input
                  [formControl]="otpCode"
                  type="text"
                  maxlength="6"
                  placeholder="Enter 6-digit code"
                  class="block w-48 rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm px-3 py-2 border">
                <button
                  (click)="enableMfa()"
                  [disabled]="otpCode.invalid || loading()"
                  class="px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-md hover:bg-green-700 disabled:opacity-50">
                  Verify &amp; Enable
                </button>
              </div>
            </div>
          </div>
        }

        <!-- Backup Codes -->
        @if (backupCodes().length > 0) {
          <div class="mt-6 bg-white rounded-lg border border-green-200 p-6">
            <h3 class="text-lg font-medium text-green-800 mb-2">MFA Enabled Successfully!</h3>
            <p class="text-sm text-gray-600 mb-4">Save these backup codes in a safe place. Each code can only be used once.</p>
            <div class="grid grid-cols-2 gap-2 bg-gray-50 p-4 rounded-md">
              @for (code of backupCodes(); track code) {
                <code class="text-sm font-mono text-gray-700">{{ code }}</code>
              }
            </div>
            <p class="text-xs text-red-500 mt-3">Warning: These codes will not be shown again.</p>
          </div>
        }
      } @else {
        <!-- MFA Enabled -->
        <div class="bg-white rounded-lg border border-green-200 p-6">
          <div class="flex items-start gap-4">
            <div class="flex-shrink-0 w-10 h-10 rounded-full bg-green-100 flex items-center justify-center">
              <svg class="w-5 h-5 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"/>
              </svg>
            </div>
            <div class="flex-1">
              <h3 class="text-lg font-medium text-green-800">Two-factor authentication is enabled</h3>
              <p class="text-sm text-gray-500 mt-1">Your account is protected with TOTP-based two-factor authentication.</p>
            </div>
          </div>

          <div class="mt-6 flex gap-3">
            <button
              (click)="regenerateBackupCodes()"
              [disabled]="loading()"
              class="px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50">
              Regenerate Backup Codes
            </button>
            <button
              (click)="showDisableConfirm.set(true)"
              class="px-4 py-2 border border-red-300 text-sm font-medium rounded-md text-red-700 bg-white hover:bg-red-50">
              Disable MFA
            </button>
          </div>

          @if (showDisableConfirm()) {
            <div class="mt-4 p-4 bg-red-50 rounded-md border border-red-200">
              <p class="text-sm text-red-700 mb-3">Enter your current TOTP code to disable MFA:</p>
              <div class="flex gap-3">
                <input
                  [formControl]="disableCode"
                  type="text"
                  maxlength="6"
                  placeholder="Enter 6-digit code"
                  class="block w-48 rounded-md border-red-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-sm px-3 py-2 border">
                <button
                  (click)="disableMfa()"
                  [disabled]="disableCode.invalid || loading()"
                  class="px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-md hover:bg-red-700 disabled:opacity-50">
                  Confirm Disable
                </button>
                <button
                  (click)="showDisableConfirm.set(false)"
                  class="px-4 py-2 text-sm text-gray-600 hover:text-gray-800">
                  Cancel
                </button>
              </div>
            </div>
          }

          @if (backupCodes().length > 0) {
            <div class="mt-4 bg-gray-50 p-4 rounded-md">
              <h4 class="text-sm font-medium text-gray-700 mb-2">New Backup Codes</h4>
              <div class="grid grid-cols-2 gap-2">
                @for (code of backupCodes(); track code) {
                  <code class="text-sm font-mono text-gray-700">{{ code }}</code>
                }
              </div>
              <p class="text-xs text-red-500 mt-3">Save these codes. They will not be shown again.</p>
            </div>
          }
        </div>
      }

      @if (error()) {
        <div class="mt-4 p-3 bg-red-50 border border-red-200 rounded-md">
          <p class="text-sm text-red-700">{{ error() }}</p>
        </div>
      }
    </div>
  `
})
export class MfaSettingsComponent {
  private http = inject(HttpClient);

  mfaEnabled = signal(false);
  loading = signal(false);
  setupData = signal<{ secretKey: string; qrCodeUri: string } | null>(null);
  backupCodes = signal<string[]>([]);
  error = signal<string | null>(null);
  showDisableConfirm = signal(false);

  otpCode = new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]);
  disableCode = new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]);

  setupMfa(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.post<{ secretKey: string; qrCodeUri: string }>('/api/v1/auth/mfa/setup', {}).subscribe({
      next: (data) => {
        this.setupData.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to setup MFA');
        this.loading.set(false);
      }
    });
  }

  enableMfa(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.post<{ backupCodes: string[] }>('/api/v1/auth/mfa/enable', { otpCode: this.otpCode.value }).subscribe({
      next: (data) => {
        this.mfaEnabled.set(true);
        this.backupCodes.set(data.backupCodes);
        this.setupData.set(null);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Invalid verification code');
        this.loading.set(false);
      }
    });
  }

  disableMfa(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.post('/api/v1/auth/mfa/disable', { otpCode: this.disableCode.value }).subscribe({
      next: () => {
        this.mfaEnabled.set(false);
        this.showDisableConfirm.set(false);
        this.backupCodes.set([]);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to disable MFA');
        this.loading.set(false);
      }
    });
  }

  regenerateBackupCodes(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.post<{ backupCodes: string[] }>('/api/v1/auth/mfa/backup-codes/regenerate', {}).subscribe({
      next: (data) => {
        this.backupCodes.set(data.backupCodes);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to regenerate codes');
        this.loading.set(false);
      }
    });
  }
}
