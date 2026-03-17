import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthActions } from '@app/store/auth/auth.actions';
import { selectAuthError, selectAuthLoading, selectIsAuthenticated } from '@app/store/auth/auth.selectors';
import { filter, take } from 'rxjs/operators';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h1 class="text-3xl font-bold text-gray-900">Welcome back</h1>
          <p class="mt-2 text-sm text-gray-600">Sign in to your account</p>
        </div>

        @if (error$ | async; as error) {
          <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
            {{ error.message }}
          </div>
        }

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="mt-8 space-y-6">
          <div class="space-y-4">
            <div>
              <label for="email" class="block text-sm font-medium text-gray-700">Email address</label>
              <input
                id="email"
                type="email"
                formControlName="email"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 sm:text-sm"
                placeholder="you@example.com"
              />
              @if (loginForm.get('email')?.touched && loginForm.get('email')?.errors?.['required']) {
                <p class="mt-1 text-xs text-red-600">Email is required</p>
              }
              @if (loginForm.get('email')?.touched && loginForm.get('email')?.errors?.['email']) {
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
                placeholder="Enter your password"
              />
              @if (loginForm.get('password')?.touched && loginForm.get('password')?.errors?.['required']) {
                <p class="mt-1 text-xs text-red-600">Password is required</p>
              }
            </div>
          </div>

          <div class="flex items-center justify-between">
            <div class="flex items-center">
              <input
                id="remember"
                type="checkbox"
                formControlName="remember"
                class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <label for="remember" class="ml-2 block text-sm text-gray-700">Remember me</label>
            </div>
            <a routerLink="/auth/forgot-password" class="text-sm font-medium text-blue-600 hover:text-blue-500">
              Forgot password?
            </a>
          </div>

          <button
            type="submit"
            [disabled]="loginForm.invalid || (loading$ | async)"
            class="w-full flex justify-center py-2.5 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            @if (loading$ | async) {
              <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
              </svg>
              Signing in...
            } @else {
              Sign in
            }
          </button>
        </form>

        <p class="text-center text-sm text-gray-600">
          Don't have an account?
          <a routerLink="/auth/register" class="font-medium text-blue-600 hover:text-blue-500">Register</a>
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
export class LoginComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  loading$ = this.store.select(selectAuthLoading);
  error$ = this.store.select(selectAuthError);

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    remember: [false],
  });

  ngOnInit(): void {
    this.store.select(selectIsAuthenticated).pipe(
      filter((isAuth) => isAuth),
      take(1),
    ).subscribe(() => {
      this.router.navigate(['/conversations']);
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    const { email, password } = this.loginForm.value;
    this.store.dispatch(AuthActions.login({ credentials: { email, password } }));
  }
}
