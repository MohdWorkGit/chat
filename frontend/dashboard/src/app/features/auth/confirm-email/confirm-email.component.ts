import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '@env/environment';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="max-w-md w-full px-6">
        <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-8 text-center">
          @if (loading) {
            <div class="flex flex-col items-center gap-4">
              <div class="h-10 w-10 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
              <p class="text-sm text-gray-500">Confirming your email address...</p>
            </div>
          } @else if (success) {
            <div class="flex flex-col items-center gap-4">
              <div class="flex h-14 w-14 items-center justify-center rounded-full bg-green-100">
                <svg class="h-7 w-7 text-green-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
                </svg>
              </div>
              <div>
                <h1 class="text-lg font-semibold text-gray-900">Email Confirmed</h1>
                <p class="mt-1 text-sm text-gray-500">Your email address has been verified successfully.</p>
              </div>
              <a
                routerLink="/auth/login"
                class="mt-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
              >
                Sign In
              </a>
            </div>
          } @else {
            <div class="flex flex-col items-center gap-4">
              <div class="flex h-14 w-14 items-center justify-center rounded-full bg-red-100">
                <svg class="h-7 w-7 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z" />
                </svg>
              </div>
              <div>
                <h1 class="text-lg font-semibold text-gray-900">Confirmation Failed</h1>
                <p class="mt-1 text-sm text-gray-500">{{ error }}</p>
              </div>
              <a
                routerLink="/auth/login"
                class="mt-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg transition-colors"
              >
                Back to Login
              </a>
            </div>
          }
        </div>
      </div>
    </div>
  `,
})
export class ConfirmEmailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly http = inject(HttpClient);

  loading = true;
  success = false;
  error = 'The confirmation link is invalid or has expired.';

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email') ?? '';
    const token = this.route.snapshot.queryParamMap.get('token') ?? '';

    if (!email || !token) {
      this.loading = false;
      this.error = 'Missing confirmation parameters. Please use the link from your email.';
      return;
    }

    this.http.post(`${environment.apiUrl}/api/v1/auth/confirm-email`, { email, token }).subscribe({
      next: () => {
        this.loading = false;
        this.success = true;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
