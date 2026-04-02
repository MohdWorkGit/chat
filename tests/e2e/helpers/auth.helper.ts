import { type Page, type APIRequestContext, expect } from '@playwright/test';

const API_URL = process.env.API_URL ?? 'http://localhost:5000';

export interface TestUser {
  email: string;
  password: string;
  accountId?: number;
  userId?: number;
  accessToken?: string;
}

export const DEFAULT_ADMIN: TestUser = {
  email: process.env.TEST_ADMIN_EMAIL ?? 'admin@test.com',
  password: process.env.TEST_ADMIN_PASSWORD ?? 'Password1!',
  accountId: 1,
};

export const DEFAULT_AGENT: TestUser = {
  email: process.env.TEST_AGENT_EMAIL ?? 'agent@test.com',
  password: process.env.TEST_AGENT_PASSWORD ?? 'Password1!',
  accountId: 1,
};

/**
 * Authenticate via the API and return an access token.
 */
export async function authenticateViaApi(
  request: APIRequestContext,
  user: TestUser = DEFAULT_ADMIN,
): Promise<string> {
  const response = await request.post(`${API_URL}/api/v1/auth/sign_in`, {
    data: {
      email: user.email,
      password: user.password,
    },
  });

  expect(response.ok(), `API login failed for ${user.email}: ${response.status()}`).toBeTruthy();
  const body = await response.json();
  return body.data?.access_token ?? body.access_token;
}

/**
 * Log in through the dashboard UI form.
 */
export async function loginViaUi(page: Page, user: TestUser = DEFAULT_ADMIN): Promise<void> {
  await page.goto('/auth/login');
  await page.getByLabel('Email').fill(user.email);
  await page.getByLabel('Password').fill(user.password);
  await page.getByRole('button', { name: /sign in|log in/i }).click();

  // Wait for redirect away from login page
  await expect(page).not.toHaveURL(/auth\/login/, { timeout: 15_000 });
}

/**
 * Log out via the UI.
 */
export async function logoutViaUi(page: Page): Promise<void> {
  // Open user/avatar menu and click logout
  await page.getByTestId('user-menu').or(page.getByRole('button', { name: /avatar|profile|account/i })).first().click();
  await page.getByRole('menuitem', { name: /log\s?out|sign\s?out/i }).click();
  await expect(page).toHaveURL(/auth\/login/);
}

/**
 * Save authenticated storage state to a file (used in global setup).
 */
export async function saveAuthState(page: Page, path: string): Promise<void> {
  await page.context().storageState({ path });
}
