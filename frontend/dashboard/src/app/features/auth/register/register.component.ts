import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthActions } from '@app/store/auth/auth.actions';
import { selectAuthError, selectAuthLoading } from '@app/store/auth/auth.selectors';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-3xl font-bold text-gray-900">Create your account</h1>
          <p class="mt-2 text-sm text-gray-600">Get started with your customer engagement platform</p>
        </div>

        @if (error$ | async; as error) {
          <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
            {{ error.message }}
          </div>
        }

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="mt-8 space-y-6">
          <div class="space-y-4">
            <div>
              <label for="name" class="block text-sm font-medium text-gray-700">Full name</label>
              <input
                id="name"
                type="text"
                formControlName="name"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 sm:text-sm"
                placeholder="John Doe"
              />
              @if (registerForm.get('name')?.touched && registerForm.get('name')?.errors?.['required']) {
                <p class="mt-1 text-xs text-red-600">Name is required</p>
              }
            </div>

            <div>
              <label for="email" class="block text-sm font-medium text-gray-700">Email address</label>
              <input
                id="email"
                type="email"
                formControlName="email"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 sm:text-sm"
                placeholder="you@example.com"
              />
              @if (registerForm.get('email')?.touched && registerForm.get('email')?.errors?.['required']) {
                <p class="mt-1 text-xs text-red-600">Email is required</p>
              }
              @if (registerForm.get('email')?.touched && registerForm.get('email')?.errors?.['email']) {
                <p class="mt-1 text-xs text-red-600">Please enter a valid email address</p>
              }
            </div>

            <div>
              <label for="password" class="block text-sm font-medium text-gray-700">Password</label>
              <input
                id="password"
                type="password"
                formControlName="password"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 sm:text-sm"
                placeholder="Create a password"
              />
              @if (registerForm.get('password')?.touched && registerForm.get('password')?.errors?.['required']) {
                <p class="mt-1 text-xs text-red-600">Password is required</p>
              }
              @if (registerForm.get('password')?.touched && registerForm.get('password')?.errors?.['minlength']) {
                <p class="mt-1 text-xs text-red-600">Password must be at least 8 characters</p>
              }

              <!-- Password strength indicator -->
              @if (registerForm.get('password')?.value) {
                <div class="mt-2">
                  <div class="flex gap-1">
                    @for (bar of [0, 1, 2, 3]; track bar) {
                      <div
                        class="h-1 flex-1 rounded-full transition-colors"
                        [class]="bar < passwordStrength ? strengthColors[passwordStrength - 1] : 'bg-gray-200'"
                      ></div>
                    }
                  </div>
                  <p class="mt-1 text-xs" [class]="strengthTextColors[passwordStrength - 1] || 'text-gray-400'">
                    @switch (passwordStrength) {
                      @case (1) { Weak }
                      @case (2) { Fair }
                      @case (3) { Good }
                      @case (4) { Strong }
                      @default { Enter a password }
                    }
                  </p>
                </div>
              }
            </div>

            <div>
              <label for="confirmPassword" class="block text-sm font-medium text-gray-700">Confirm password</label>
              <input
                id="confirmPassword"
                type="password"
                formControlName="confirmPassword"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 sm:text-sm"
                placeholder="Confirm your password"
              />
              @if (registerForm.get('confirmPassword')?.touched && registerForm.get('confirmPassword')?.errors?.['required']) {
                <p class="mt-1 text-xs text-red-600">Please confirm your password</p>
              }
              @if (registerForm.get('confirmPassword')?.touched && registerForm.errors?.['passwordMismatch']) {
                <p class="mt-1 text-xs text-red-600">Passwords do not match</p>
              }
            </div>
          </div>

          <button
            type="submit"
            [disabled]="registerForm.invalid || (loading$ | async)"
            class="w-full flex justify-center py-2.5 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            @if (loading$ | async) {
              <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
              </svg>
              Creating account...
            } @else {
              Create account
            }
          </button>
        </form>

        <p class="text-center text-sm text-gray-600">
          Already have an account?
          <a routerLink="/auth/login" class="font-medium text-blue-600 hover:text-blue-500">Sign in</a>
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
export class RegisterComponent {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  loading$ = this.store.select(selectAuthLoading);
  error$ = this.store.select(selectAuthError);

  strengthColors = ['bg-red-500', 'bg-orange-500', 'bg-yellow-500', 'bg-green-500'];
  strengthTextColors = ['text-red-600', 'text-orange-600', 'text-yellow-600', 'text-green-600'];

  registerForm: FormGroup = this.fb.group(
    {
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: this.passwordMatchValidator },
  );

  get passwordStrength(): number {
    const password = this.registerForm.get('password')?.value || '';
    if (!password) return 0;
    let strength = 0;
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
    if (/\d/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++;
    return strength;
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    const { name, email, password, confirmPassword } = this.registerForm.value;
    this.store.dispatch(
      AuthActions.register({ data: { name, email, password, confirmPassword } }),
    );
  }
}
