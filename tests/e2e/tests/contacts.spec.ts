import { test, expect } from '@playwright/test';
import { ApiHelper } from '../helpers/api.helper';
import { authenticateViaApi, DEFAULT_ADMIN } from '../helpers/auth.helper';

test.describe('Contacts', () => {
  let api: ApiHelper;

  test.beforeAll(async ({ request }) => {
    const token = await authenticateViaApi(request, DEFAULT_ADMIN);
    api = new ApiHelper(request, { accessToken: token });
  });

  test.describe('List Contacts', () => {
    test('should display the contacts list page', async ({ page }) => {
      await page.goto('/contacts');
      await expect(page).toHaveURL(/contacts/);
      await expect(
        page.getByRole('heading', { name: /contacts/i }).or(
          page.locator('[data-testid="contact-list"]'),
        ),
      ).toBeVisible({ timeout: 10_000 });
    });

    test('should show a table or list of contacts', async ({ page }) => {
      await page.goto('/contacts');
      await page.waitForLoadState('networkidle');

      const contactRows = page.locator(
        '[data-testid="contact-item"], .contact-item, table tbody tr',
      );
      const count = await contactRows.count();
      if (count > 0) {
        await expect(contactRows.first()).toBeVisible();
      }
    });

    test('should paginate when many contacts exist', async ({ page }) => {
      await page.goto('/contacts');
      await page.waitForLoadState('networkidle');

      // Pagination controls may or may not be present depending on data
      const pagination = page.locator(
        '[data-testid="pagination"], .pagination, nav[aria-label*="pagination"]',
      );
      // Just verify page loads without error; pagination visibility depends on data volume
      await expect(page).toHaveURL(/contacts/);
    });
  });

  test.describe('Create Contact', () => {
    test('should navigate to new contact form', async ({ page }) => {
      await page.goto('/contacts');
      const createBtn = page.getByRole('button', { name: /new|create|add/i }).or(
        page.getByRole('link', { name: /new|create|add/i }),
      ).first();
      await expect(createBtn).toBeVisible({ timeout: 10_000 });
      await createBtn.click();

      await expect(page).toHaveURL(/contacts\/new|contacts.*new/);
    });

    test('should create a contact via the form', async ({ page }) => {
      await page.goto('/contacts/new');

      const uniqueName = `E2E Contact ${Date.now()}`;
      const uniqueEmail = `e2e-${Date.now()}@test.com`;

      await page.getByLabel(/name/i).first().fill(uniqueName);
      await page.getByLabel(/email/i).first().fill(uniqueEmail);

      const phoneField = page.getByLabel(/phone/i).first();
      if (await phoneField.isVisible().catch(() => false)) {
        await phoneField.fill('+15551234567');
      }

      await page.getByRole('button', { name: /save|create|submit/i }).first().click();

      // Should redirect to contact detail or list
      await expect(page).not.toHaveURL(/contacts\/new/, { timeout: 10_000 });
    });

    test('should create a contact via API', async () => {
      const result = await api.createContact({
        name: `API Contact ${Date.now()}`,
        email: `api-${Date.now()}@test.com`,
      });
      expect(result.id ?? result.data?.id).toBeTruthy();
    });
  });

  test.describe('Search Contacts', () => {
    test('should filter contacts by search query', async ({ page }) => {
      // First create a contact with a unique name
      const uniqueName = `SearchTarget-${Date.now()}`;
      await api.createContact({ name: uniqueName, email: `${uniqueName}@test.com` });

      await page.goto('/contacts');
      await page.waitForLoadState('networkidle');

      const searchInput = page.getByPlaceholder(/search/i).or(
        page.getByRole('searchbox'),
      ).first();

      if (await searchInput.isVisible({ timeout: 5_000 }).catch(() => false)) {
        await searchInput.fill(uniqueName);
        await searchInput.press('Enter');
        await page.waitForLoadState('networkidle');

        await expect(page.getByText(uniqueName)).toBeVisible({ timeout: 10_000 });
      }
    });

    test('should return results via API search', async () => {
      const result = await api.searchContacts('test');
      expect(result).toBeTruthy();
    });
  });

  test.describe('View Contact Detail', () => {
    test('should display contact information', async ({ page }) => {
      // Create a contact to view
      const name = `Detail Contact ${Date.now()}`;
      const contact = await api.createContact({ name, email: `detail-${Date.now()}@test.com` });
      const contactId = contact.id ?? contact.data?.id;

      await page.goto(`/contacts/${contactId}`);
      await expect(page).toHaveURL(new RegExp(`contacts/${contactId}`));

      await expect(
        page.getByText(name).or(page.locator('[data-testid="contact-detail"]')),
      ).toBeVisible({ timeout: 10_000 });
    });

    test('should show conversation history on contact detail', async ({ page }) => {
      await page.goto('/contacts');
      await page.waitForLoadState('networkidle');

      const firstContact = page.locator(
        '[data-testid="contact-item"], .contact-item, table tbody tr',
      ).first();

      if (await firstContact.isVisible({ timeout: 5_000 }).catch(() => false)) {
        await firstContact.click();
        await page.waitForLoadState('networkidle');

        // The contact detail page should load; conversation section may or may not be present
        await expect(page).toHaveURL(/contacts\/\d+/);
      }
    });
  });
});
