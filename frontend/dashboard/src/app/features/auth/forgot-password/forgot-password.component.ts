import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthActions } from '@app/store/auth/auth.actions';
import { selectAuthError, selectAuthLoading, selectForgotPasswordSuccess } from '@app/store/auth/auth.selectors';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-3xl font-bold text-gray-900">Reset your password</h1>
          <p class="mt-2 text-sm text-gray-600">
            Enter your email address and we'll send you a link to reset your password.
          </p>
        </div>

        @if (success$ | async) {
          <div class="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-lg text-sm">
            <p class="font-medium">Check your email</p>
            <p class="mt-1">If an account exists with that email, we've sent password reset instructions.</p>
          </div>
        }

        @if (error$ | async; as error) {
          <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
            {{ error.message }}
          </div>
        }

        <form [formGroup]="forgotForm" (ngSubmit)="onSubmit()" class="mt-8 space-y-6">
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700">Email address</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 sm:text-sm"
              placeholder="you@example.com"
            />
            @if (forgotForm.get('email')?.touched && forgotForm.get('email')?.errors?.['required']) {
              <p class="mt-1 text-xs text-red-600">Email is required</p>
            }
            @if (forgotForm.get('email')?.touched && forgotForm.get('email')?.errors?.['email']) {
              <p class="mt-1 text-xs text-red-600">Please enter a valid email address</p>
            }
          </div>

          <button
            type="submit"
            [disabled]="forgotForm.invalid || (loading$ | async)"
            class="w-full flex justify-center py-2.5 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            @if (loading$ | async) {
              <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
              </svg>
              Sending...
            } @else {
              Send reset link
            }
          </button>
        </form>

        <p class="text-center text-sm text-gray-600">
          <a routerLink="/auth/login" class="font-medium text-blue-600 hover:text-blue-500">
            Back to sign in
          </a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class ForgotPasswordComponent {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  loading$ = this.store.select(selectAuthLoading);
  error$ = this.store.select(selectAuthError);
  success$ = this.store.select(selectForgotPasswordSuccess);

  forgotForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  onSubmit(): void {
    if (this.forgotForm.invalid) return;

    const { email } = this.forgotForm.value;
    this.store.dispatch(AuthActions.forgotPassword({ email }));
  }
}
