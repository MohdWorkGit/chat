import { defineConfig, devices } from '@playwright/test';

const DASHBOARD_URL = process.env.DASHBOARD_URL ?? 'http://localhost:4200';
const API_URL = process.env.API_URL ?? 'http://localhost:5000';
const PORTAL_URL = process.env.PORTAL_URL ?? 'http://localhost:4300';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: process.env.CI
    ? [['html', { open: 'never' }], ['junit', { outputFile: 'results/junit.xml' }]]
    : [['html', { open: 'on-failure' }]],

  use: {
    baseURL: DASHBOARD_URL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    actionTimeout: 10_000,
    navigationTimeout: 15_000,
  },

  projects: [
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: 'playwright/.auth/user.json',
      },
      dependencies: ['setup'],
    },
    {
      name: 'firefox',
      use: {
        ...devices['Desktop Firefox'],
        storageState: 'playwright/.auth/user.json',
      },
      dependencies: ['setup'],
    },
    {
      name: 'webkit',
      use: {
        ...devices['Desktop Safari'],
        storageState: 'playwright/.auth/user.json',
      },
      dependencies: ['setup'],
    },
    {
      name: 'widget',
      testMatch: /widget\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
      },
    },
  ],

  /* Configure web servers to start before tests if desired */
  // webServer: [
  //   {
  //     command: 'cd ../../frontend/dashboard && npm start',
  //     url: DASHBOARD_URL,
  //     reuseExistingServer: !process.env.CI,
  //     timeout: 120_000,
  //   },
  //   {
  //     command: 'cd ../../src/CustomerEngagement.Api && dotnet run',
  //     url: `${API_URL}/api/v1/auth`,
  //     reuseExistingServer: !process.env.CI,
  //     timeout: 120_000,
  //   },
  // ],
});

export { DASHBOARD_URL, API_URL, PORTAL_URL };
