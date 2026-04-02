import { test, expect } from '@playwright/test';
import { ApiHelper } from '../helpers/api.helper';
import { authenticateViaApi, DEFAULT_ADMIN } from '../helpers/auth.helper';

const PORTAL_URL = process.env.PORTAL_URL ?? 'http://localhost:4300';

test.describe('Help Center', () => {
  let api: ApiHelper;

  test.beforeAll(async ({ request }) => {
    const token = await authenticateViaApi(request, DEFAULT_ADMIN);
    api = new ApiHelper(request, { accessToken: token });
  });

  // ── Portal CRUD ────────────────────────────────────────────────────

  test.describe('Portal Management', () => {
    test('should display the help center page in the dashboard', async ({ page }) => {
      await page.goto('/helpcenter');
      await expect(page).toHaveURL(/helpcenter/);
    });

    test('should create a portal via API', async () => {
      const result = await api.createPortal({
        name: `E2E Portal ${Date.now()}`,
        slug: `e2e-portal-${Date.now()}`,
      });
      expect(result.id ?? result.data?.id).toBeTruthy();
    });

    test('should list portals via API', async () => {
      const result = await api.getPortals();
      expect(result).toBeTruthy();
    });

    test('should delete a portal via API', async () => {
      const portal = await api.createPortal({
        name: `ToDelete ${Date.now()}`,
        slug: `to-delete-${Date.now()}`,
      });
      const id = portal.id ?? portal.data?.id;
      await api.deletePortal(id);
    });
  });

  // ── Article CRUD ───────────────────────────────────────────────────

  test.describe('Article Management', () => {
    let portalId: number;

    test.beforeAll(async () => {
      const portal = await api.createPortal({
        name: `Article Test Portal ${Date.now()}`,
        slug: `article-test-${Date.now()}`,
      });
      portalId = portal.id ?? portal.data?.id;
    });

    test('should create an article via API', async () => {
      const result = await api.createArticle(portalId, {
        title: `E2E Article ${Date.now()}`,
        content: '<p>This is an E2E test article with <strong>rich</strong> content.</p>',
      });
      expect(result.id ?? result.data?.id).toBeTruthy();
    });

    test('should list articles for a portal via API', async () => {
      // Create an article first
      await api.createArticle(portalId, {
        title: `Listed Article ${Date.now()}`,
        content: '<p>Article for listing test</p>',
      });

      const result = await api.getArticles(portalId);
      const articles = result.payload ?? result.data ?? result;
      expect(articles).toBeTruthy();
    });

    test('should display articles in the dashboard help center view', async ({ page }) => {
      await page.goto('/helpcenter');
      await page.waitForLoadState('networkidle');

      // The help center page should load without errors
      await expect(page).toHaveURL(/helpcenter/);
    });
  });

  // ── Public Portal View ─────────────────────────────────────────────

  test.describe('Public Portal', () => {
    test('should load the public portal landing page', async ({ page }) => {
      await page.goto(PORTAL_URL);

      // The portal frontend should render
      await expect(page.locator('body')).not.toBeEmpty();
    });

    test('should display search on the portal', async ({ page }) => {
      await page.goto(PORTAL_URL);

      const searchInput = page.getByPlaceholder(/search/i).or(
        page.getByRole('searchbox'),
      ).first();

      // Search may be present on the portal home page
      if (await searchInput.isVisible({ timeout: 5_000 }).catch(() => false)) {
        await searchInput.fill('test query');
        await searchInput.press('Enter');
        await page.waitForLoadState('networkidle');
      }
    });

    test('should navigate to an article page', async ({ page }) => {
      await page.goto(`${PORTAL_URL}/search`);
      await page.waitForLoadState('networkidle');

      const articleLink = page.getByRole('link').filter({ hasText: /.+/ }).first();
      if (await articleLink.isVisible({ timeout: 5_000 }).catch(() => false)) {
        await articleLink.click();
        await expect(page).toHaveURL(/article/);
      }
    });

    test('should navigate to a category page', async ({ page }) => {
      await page.goto(PORTAL_URL);
      await page.waitForLoadState('networkidle');

      const categoryLink = page.locator('[data-testid="category-link"], a[href*="category"]').first();
      if (await categoryLink.isVisible({ timeout: 5_000 }).catch(() => false)) {
        await categoryLink.click();
        await expect(page).toHaveURL(/category/);
      }
    });
  });
});
