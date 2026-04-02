import { type APIRequestContext, expect } from '@playwright/test';

const API_URL = process.env.API_URL ?? 'http://localhost:5000';

export interface ApiHelperOptions {
  accountId?: number;
  accessToken: string;
}

/**
 * Convenience wrapper around Playwright APIRequestContext for the Customer
 * Engagement REST API. All methods automatically set auth headers and the
 * account-scoped base path.
 */
export class ApiHelper {
  private readonly baseUrl: string;
  private readonly token: string;

  constructor(
    private readonly request: APIRequestContext,
    options: ApiHelperOptions,
  ) {
    const accountId = options.accountId ?? 1;
    this.baseUrl = `${API_URL}/api/v1/accounts/${accountId}`;
    this.token = options.accessToken;
  }

  private headers() {
    return {
      Authorization: `Bearer ${this.token}`,
      'Content-Type': 'application/json',
    };
  }

  // ── Conversations ──────────────────────────────────────────────────

  async getConversations(params?: Record<string, string>) {
    const response = await this.request.get(`${this.baseUrl}/conversations`, {
      headers: this.headers(),
      params,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async createConversation(data: {
    inbox_id: number;
    contact_id: number;
    message?: { content: string };
  }) {
    const response = await this.request.post(`${this.baseUrl}/conversations`, {
      headers: this.headers(),
      data,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async sendMessage(conversationId: number, content: string) {
    const response = await this.request.post(
      `${this.baseUrl}/conversations/${conversationId}/messages`,
      {
        headers: this.headers(),
        data: { content, message_type: 'outgoing' },
      },
    );
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async updateConversationStatus(conversationId: number, status: string) {
    const response = await this.request.post(
      `${this.baseUrl}/conversations/${conversationId}/toggle_status`,
      {
        headers: this.headers(),
        data: { status },
      },
    );
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  // ── Contacts ───────────────────────────────────────────────────────

  async getContacts(params?: Record<string, string>) {
    const response = await this.request.get(`${this.baseUrl}/contacts`, {
      headers: this.headers(),
      params,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async createContact(data: { name: string; email?: string; phone_number?: string }) {
    const response = await this.request.post(`${this.baseUrl}/contacts`, {
      headers: this.headers(),
      data,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async searchContacts(query: string) {
    const response = await this.request.get(`${this.baseUrl}/search`, {
      headers: this.headers(),
      params: { q: query },
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  // ── Inboxes ────────────────────────────────────────────────────────

  async getInboxes() {
    const response = await this.request.get(`${this.baseUrl}/inboxes`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async createInbox(data: { name: string; channel?: { type: string } }) {
    const response = await this.request.post(`${this.baseUrl}/inboxes`, {
      headers: this.headers(),
      data,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async deleteInbox(inboxId: number) {
    const response = await this.request.delete(`${this.baseUrl}/inboxes/${inboxId}`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
  }

  // ── Teams ──────────────────────────────────────────────────────────

  async getTeams() {
    const response = await this.request.get(`${this.baseUrl}/teams`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async createTeam(data: { name: string; description?: string }) {
    const response = await this.request.post(`${this.baseUrl}/teams`, {
      headers: this.headers(),
      data,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async deleteTeam(teamId: number) {
    const response = await this.request.delete(`${this.baseUrl}/teams/${teamId}`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
  }

  // ── Labels ─────────────────────────────────────────────────────────

  async getLabels() {
    const response = await this.request.get(`${this.baseUrl}/labels`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async createLabel(data: { title: string; description?: string; color?: string }) {
    const response = await this.request.post(`${this.baseUrl}/labels`, {
      headers: this.headers(),
      data,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async deleteLabel(labelId: number) {
    const response = await this.request.delete(`${this.baseUrl}/labels/${labelId}`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
  }

  // ── Canned Responses ──────────────────────────────────────────────

  async getCannedResponses() {
    const response = await this.request.get(`${this.baseUrl}/canned_responses`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async createCannedResponse(data: { short_code: string; content: string }) {
    const response = await this.request.post(`${this.baseUrl}/canned_responses`, {
      headers: this.headers(),
      data,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  // ── Portals & Articles ────────────────────────────────────────────

  async getPortals() {
    const response = await this.request.get(`${API_URL}/api/v1/portals`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async createPortal(data: {
    name: string;
    slug: string;
    color?: string;
    homepage_link?: string;
  }) {
    const response = await this.request.post(`${API_URL}/api/v1/portals`, {
      headers: this.headers(),
      data,
    });
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async deletePortal(portalId: number) {
    const response = await this.request.delete(`${API_URL}/api/v1/portals/${portalId}`, {
      headers: this.headers(),
    });
    expect(response.ok()).toBeTruthy();
  }

  async getArticles(portalId: number) {
    const response = await this.request.get(
      `${API_URL}/api/v1/portals/${portalId}/articles`,
      { headers: this.headers() },
    );
    expect(response.ok()).toBeTruthy();
    return response.json();
  }

  async createArticle(
    portalId: number,
    data: { title: string; content: string; category_id?: number },
  ) {
    const response = await this.request.post(
      `${API_URL}/api/v1/portals/${portalId}/articles`,
      { headers: this.headers(), data },
    );
    expect(response.ok()).toBeTruthy();
    return response.json();
  }
}
