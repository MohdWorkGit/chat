import { test, expect } from '@playwright/test';
import { DEFAULT_ADMIN, loginViaUi, logoutViaUi } from '../helpers/auth.helper';

test.describe('Authentication', () => {
  test.use({ storageState: { cookies: [], origins: [] } }); // unauthenticated

  test.describe('Login', () => {
    test('should display the login page', async ({ page }) => {
      await page.goto('/auth/login');
      await expect(page.getByRole('heading', { name: /sign in|log in/i })).toBeVisible();
      await expect(page.getByLabel('Email')).toBeVisible();
      await expect(page.getByLabel('Password')).toBeVisible();
    });

    test('should reject invalid credentials', async ({ page }) => {
      await page.goto('/auth/login');
      await page.getByLabel('Email').fill('wrong@example.com');
      await page.getByLabel('Password').fill('WrongPassword123!');
      await page.getByRole('button', { name: /sign in|log in/i }).click();

      await expect(
        page.getByText(/invalid|incorrect|unauthorized/i),
      ).toBeVisible({ timeout: 10_000 });
    });

    test('should log in with valid credentials and redirect to dashboard', async ({ page }) => {
      await loginViaUi(page, DEFAULT_ADMIN);
      await expect(page).toHaveURL(/\/(conversations|dashboard)?$/);
    });

    test('should show password visibility toggle', async ({ page }) => {
      await page.goto('/auth/login');
      const passwordField = page.getByLabel('Password');
      await expect(passwordField).toHaveAttribute('type', 'password');

      const toggle = page.getByRole('button', { name: /show|toggle|visibility/i });
      if (await toggle.isVisible()) {
        await toggle.click();
        await expect(passwordField).toHaveAttribute('type', 'text');
      }
    });
  });

  test.describe('Registration', () => {
    test('should display the registration page', async ({ page }) => {
      await page.goto('/auth/register');
      await expect(page.getByLabel('Email')).toBeVisible();
      await expect(page.getByRole('button', { name: /sign up|register|create/i })).toBeVisible();
    });

    test('should validate required fields', async ({ page }) => {
      await page.goto('/auth/register');
      await page.getByRole('button', { name: /sign up|register|create/i }).click();

      // Expect validation messages for required fields
      const validationMessages = page.locator('[class*="error"], [class*="invalid"], .mat-error, .field-error');
      await expect(validationMessages.first()).toBeVisible({ timeout: 5_000 });
    });

    test('should reject weak passwords', async ({ page }) => {
      await page.goto('/auth/register');
      await page.getByLabel('Full Name').or(page.getByLabel('Name')).first().fill('Test User');
      await page.getByLabel('Email').fill('newuser@example.com');
      await page.getByLabel('Password').first().fill('123');

      await page.getByRole('button', { name: /sign up|register|create/i }).click();
      await expect(
        page.getByText(/password|weak|short|minimum/i),
      ).toBeVisible({ timeout: 5_000 });
    });
  });

  test.describe('Forgot Password', () => {
    test('should display the forgot password page', async ({ page }) => {
      await page.goto('/auth/forgot-password');
      await expect(page.getByLabel('Email')).toBeVisible();
      await expect(
        page.getByRole('button', { name: /reset|send|submit/i }),
      ).toBeVisible();
    });

    test('should submit reset request and show confirmation', async ({ page }) => {
      await page.goto('/auth/forgot-password');
      await page.getByLabel('Email').fill(DEFAULT_ADMIN.email);
      await page.getByRole('button', { name: /reset|send|submit/i }).click();

      await expect(
        page.getByText(/sent|check your email|reset link/i),
      ).toBeVisible({ timeout: 10_000 });
    });

    test('should have a link back to login', async ({ page }) => {
      await page.goto('/auth/forgot-password');
      const backLink = page.getByRole('link', { name: /login|sign in|back/i });
      await expect(backLink).toBeVisible();
    });
  });

  test.describe('Logout', () => {
    test('should log out and redirect to login page', async ({ page }) => {
      // First login
      await loginViaUi(page, DEFAULT_ADMIN);
      await expect(page).not.toHaveURL(/auth\/login/);

      // Then logout
      await logoutViaUi(page);
      await expect(page).toHaveURL(/auth\/login/);
    });
  });
});
