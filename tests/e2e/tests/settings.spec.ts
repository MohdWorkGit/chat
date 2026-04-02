import { test, expect } from '@playwright/test';
import { ApiHelper } from '../helpers/api.helper';
import { authenticateViaApi, DEFAULT_ADMIN } from '../helpers/auth.helper';

test.describe('Settings', () => {
  let api: ApiHelper;

  test.beforeAll(async ({ request }) => {
    const token = await authenticateViaApi(request, DEFAULT_ADMIN);
    api = new ApiHelper(request, { accessToken: token });
  });

  // ── Inboxes CRUD ───────────────────────────────────────────────────

  test.describe('Inboxes', () => {
    test('should display the inbox list', async ({ page }) => {
      await page.goto('/settings/inboxes');
      await expect(page).toHaveURL(/settings\/inboxes/);
      await expect(
        page.getByRole('heading', { name: /inboxes/i }).or(
          page.locator('[data-testid="inbox-list"]'),
        ),
      ).toBeVisible({ timeout: 10_000 });
    });

    test('should navigate to create inbox form', async ({ page }) => {
      await page.goto('/settings/inboxes');
      const addBtn = page.getByRole('button', { name: /add|new|create/i }).or(
        page.getByRole('link', { name: /add|new|create/i }),
      ).first();
      await expect(addBtn).toBeVisible();
      await addBtn.click();
      await expect(page).toHaveURL(/settings\/inboxes\/new/);
    });

    test('should create an inbox via API and see it in the list', async ({ page }) => {
      const name = `E2E Inbox ${Date.now()}`;
      const inbox = await api.createInbox({ name, channel: { type: 'web_widget' } });
      const inboxId = inbox.id ?? inbox.data?.id;
      expect(inboxId).toBeTruthy();

      await page.goto('/settings/inboxes');
      await page.waitForLoadState('networkidle');
      await expect(page.getByText(name)).toBeVisible({ timeout: 10_000 });
    });

    test('should open inbox detail/edit page', async ({ page }) => {
      const inboxes = await api.getInboxes();
      const list = inboxes.payload ?? inboxes.data ?? inboxes;
      if (!Array.isArray(list) || list.length === 0) {
        test.skip(true, 'No inboxes to view');
        return;
      }

      await page.goto(`/settings/inboxes/${list[0].id}`);
      await expect(page).toHaveURL(new RegExp(`settings/inboxes/${list[0].id}`));
    });

    test('should delete an inbox via API', async () => {
      const inbox = await api.createInbox({ name: `ToDelete ${Date.now()}` });
      const id = inbox.id ?? inbox.data?.id;
      await api.deleteInbox(id);

      // Verify it no longer appears
      const remaining = await api.getInboxes();
      const list = remaining.payload ?? remaining.data ?? remaining;
      const found = Array.isArray(list) ? list.find((i: any) => i.id === id) : null;
      expect(found).toBeFalsy();
    });
  });

  // ── Teams CRUD ─────────────────────────────────────────────────────

  test.describe('Teams', () => {
    test('should display the team list', async ({ page }) => {
      await page.goto('/settings/teams');
      await expect(page).toHaveURL(/settings\/teams/);
    });

    test('should create a team via API', async () => {
      const result = await api.createTeam({
        name: `E2E Team ${Date.now()}`,
        description: 'Created by E2E test',
      });
      expect(result.id ?? result.data?.id).toBeTruthy();
    });

    test('should show newly created team in the list', async ({ page }) => {
      const name = `Visible Team ${Date.now()}`;
      await api.createTeam({ name });

      await page.goto('/settings/teams');
      await page.waitForLoadState('networkidle');
      await expect(page.getByText(name)).toBeVisible({ timeout: 10_000 });
    });

    test('should delete a team via API', async () => {
      const team = await api.createTeam({ name: `ToDelete ${Date.now()}` });
      const id = team.id ?? team.data?.id;
      await api.deleteTeam(id);

      const remaining = await api.getTeams();
      const list = remaining.payload ?? remaining.data ?? remaining;
      const found = Array.isArray(list) ? list.find((t: any) => t.id === id) : null;
      expect(found).toBeFalsy();
    });
  });

  // ── Labels CRUD ────────────────────────────────────────────────────

  test.describe('Labels', () => {
    test('should display the label list', async ({ page }) => {
      await page.goto('/settings/labels');
      await expect(page).toHaveURL(/settings\/labels/);
    });

    test('should create a label via API', async () => {
      const result = await api.createLabel({
        title: `e2e-label-${Date.now()}`,
        description: 'Created by E2E test',
        color: '#4CAF50',
      });
      expect(result.id ?? result.data?.id).toBeTruthy();
    });

    test('should show newly created label in the list', async ({ page }) => {
      const title = `visible-label-${Date.now()}`;
      await api.createLabel({ title });

      await page.goto('/settings/labels');
      await page.waitForLoadState('networkidle');
      await expect(page.getByText(title)).toBeVisible({ timeout: 10_000 });
    });

    test('should delete a label via API', async () => {
      const label = await api.createLabel({ title: `to-delete-${Date.now()}` });
      const id = label.id ?? label.data?.id;
      await api.deleteLabel(id);

      const remaining = await api.getLabels();
      const list = remaining.payload ?? remaining.data ?? remaining;
      const found = Array.isArray(list) ? list.find((l: any) => l.id === id) : null;
      expect(found).toBeFalsy();
    });
  });

  // ── Canned Responses ──────────────────────────────────────────────

  test.describe('Canned Responses', () => {
    test('should display the canned responses page', async ({ page }) => {
      await page.goto('/settings/canned-responses');
      await expect(page).toHaveURL(/settings\/canned-responses/);
    });

    test('should navigate to create canned response form', async ({ page }) => {
      await page.goto('/settings/canned-responses');
      const addBtn = page.getByRole('button', { name: /add|new|create/i }).or(
        page.getByRole('link', { name: /add|new|create/i }),
      ).first();

      if (await addBtn.isVisible({ timeout: 5_000 }).catch(() => false)) {
        await addBtn.click();
        await expect(page).toHaveURL(/canned-responses\/new/);
      }
    });

    test('should create a canned response via API', async () => {
      const result = await api.createCannedResponse({
        short_code: `e2e_${Date.now()}`,
        content: 'Thank you for contacting us! This is an E2E test response.',
      });
      expect(result.id ?? result.data?.id).toBeTruthy();
    });

    test('should list canned responses via API', async () => {
      const result = await api.getCannedResponses();
      const list = result.payload ?? result.data ?? result;
      expect(Array.isArray(list)).toBeTruthy();
    });
  });
});
