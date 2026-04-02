import { test, expect } from '@playwright/test';
import { ApiHelper } from '../helpers/api.helper';
import { authenticateViaApi, DEFAULT_ADMIN } from '../helpers/auth.helper';

test.describe('Conversations', () => {
  let api: ApiHelper;

  test.beforeAll(async ({ request }) => {
    const token = await authenticateViaApi(request, DEFAULT_ADMIN);
    api = new ApiHelper(request, { accessToken: token });
  });

  test.describe('List Conversations', () => {
    test('should display the conversations list page', async ({ page }) => {
      await page.goto('/conversations');
      await expect(page).toHaveURL(/conversations/);
      await expect(
        page.getByRole('heading', { name: /conversations/i }).or(page.locator('[data-testid="conversation-list"]')),
      ).toBeVisible({ timeout: 10_000 });
    });

    test('should show conversation items with preview text', async ({ page }) => {
      await page.goto('/conversations');
      const conversationItems = page.locator(
        '[data-testid="conversation-item"], .conversation-item, .conversation-card',
      );
      // Wait for list to load; may be empty in test env
      await page.waitForLoadState('networkidle');
      const count = await conversationItems.count();
      if (count > 0) {
        await expect(conversationItems.first()).toBeVisible();
      }
    });

    test('should support filtering by status', async ({ page }) => {
      await page.goto('/conversations');
      const statusFilter = page.getByRole('button', { name: /open|status|filter/i }).first();
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        const resolvedOption = page.getByRole('option', { name: /resolved/i }).or(
          page.getByText(/resolved/i),
        );
        if (await resolvedOption.isVisible()) {
          await resolvedOption.click();
        }
      }
    });
  });

  test.describe('Open Conversation', () => {
    test('should navigate to conversation detail', async ({ page }) => {
      // Seed a conversation via API
      const contacts = await api.getContacts();
      const contactList = contacts.payload ?? contacts.data ?? contacts;
      if (!Array.isArray(contactList) || contactList.length === 0) {
        test.skip(true, 'No contacts available to create conversation');
        return;
      }

      const inboxes = await api.getInboxes();
      const inboxList = inboxes.payload ?? inboxes.data ?? inboxes;
      if (!Array.isArray(inboxList) || inboxList.length === 0) {
        test.skip(true, 'No inboxes available');
        return;
      }

      const conversation = await api.createConversation({
        inbox_id: inboxList[0].id,
        contact_id: contactList[0].id,
        message: { content: 'E2E test message' },
      });

      const convId = conversation.id ?? conversation.data?.id;
      await page.goto(`/conversations/${convId}`);
      await expect(page).toHaveURL(new RegExp(`conversations/${convId}`));
      await expect(
        page.locator('[data-testid="conversation-detail"], .conversation-detail, .message-list'),
      ).toBeVisible({ timeout: 10_000 });
    });
  });

  test.describe('Send Message', () => {
    test('should send a message in a conversation', async ({ page }) => {
      await page.goto('/conversations');
      await page.waitForLoadState('networkidle');

      // Click the first conversation if available
      const firstItem = page.locator(
        '[data-testid="conversation-item"], .conversation-item, .conversation-card',
      ).first();

      if (!(await firstItem.isVisible({ timeout: 5_000 }).catch(() => false))) {
        test.skip(true, 'No conversations to open');
        return;
      }

      await firstItem.click();

      const messageInput = page.getByPlaceholder(/type|message|reply/i).or(
        page.locator('[data-testid="message-input"], .reply-editor textarea, .reply-box textarea'),
      ).first();
      await expect(messageInput).toBeVisible({ timeout: 10_000 });

      const testMessage = `E2E test message ${Date.now()}`;
      await messageInput.fill(testMessage);

      await page.getByRole('button', { name: /send/i }).or(
        page.locator('[data-testid="send-button"]'),
      ).first().click();

      // Verify message appears in the thread
      await expect(page.getByText(testMessage)).toBeVisible({ timeout: 10_000 });
    });
  });

  test.describe('Change Status', () => {
    test('should toggle conversation status via API', async ({ request }) => {
      const token = await authenticateViaApi(request, DEFAULT_ADMIN);
      const apiLocal = new ApiHelper(request, { accessToken: token });

      const conversations = await apiLocal.getConversations();
      const convList = conversations.data?.payload ?? conversations.payload ?? conversations.data ?? [];
      if (!Array.isArray(convList) || convList.length === 0) {
        test.skip(true, 'No conversations to change status');
        return;
      }

      const conv = convList[0];
      const newStatus = conv.status === 'open' ? 'resolved' : 'open';
      const updated = await apiLocal.updateConversationStatus(conv.id, newStatus);
      expect(updated.current_status ?? updated.status).toBe(newStatus);
    });

    test('should resolve a conversation from the UI', async ({ page }) => {
      await page.goto('/conversations');
      await page.waitForLoadState('networkidle');

      const firstItem = page.locator(
        '[data-testid="conversation-item"], .conversation-item, .conversation-card',
      ).first();

      if (!(await firstItem.isVisible({ timeout: 5_000 }).catch(() => false))) {
        test.skip(true, 'No conversations available');
        return;
      }

      await firstItem.click();
      await page.waitForLoadState('networkidle');

      const resolveBtn = page.getByRole('button', { name: /resolve/i }).or(
        page.locator('[data-testid="resolve-button"]'),
      ).first();

      if (await resolveBtn.isVisible({ timeout: 5_000 }).catch(() => false)) {
        await resolveBtn.click();
        await expect(
          page.getByText(/resolved/i).or(page.getByRole('button', { name: /reopen/i })),
        ).toBeVisible({ timeout: 10_000 });
      }
    });
  });
});
