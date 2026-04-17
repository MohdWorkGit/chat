import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CsatData } from '../components/csat-survey/csat-survey.component';

export interface Message {
  id: number;
  conversationId: number;
  content: string;
  senderType: 'agent' | 'customer';
  contentType: string;
  createdAt: string;
}

export interface Conversation {
  id: number;
  status: string;
  inboxId: number;
  contactId: number;
  messages: Message[];
}

export interface CreateConversationRequest {
  name: string;
  email: string;
  customFields?: Record<string, string>;
}

export interface WidgetConfig {
  websiteToken: string;
  inboxName: string;
  welcomeMessage: string;
  preChatFormEnabled: boolean;
  primaryColor: string;
}

@Injectable({ providedIn: 'root' })
export class WidgetApiService {
  private apiOrigin = '';

  constructor(private readonly http: HttpClient) {}

  /**
   * Configures the origin (scheme + host[:port]) of the backend API.
   * When the widget is embedded on a different domain than the API,
   * the host page must provide this via the `api-base-url` attribute so
   * requests can reach the API instead of the CDN / static host.
   */
  setApiOrigin(origin: string): void {
    // Strip trailing slashes to avoid "//api/v1/..." when concatenated.
    this.apiOrigin = (origin || '').replace(/\/+$/, '');
  }

  private get baseUrl(): string {
    return `${this.apiOrigin}/api/v1/widget`;
  }

  createConversation(websiteToken: string, data: CreateConversationRequest): Observable<Conversation> {
    return this.http.post<Conversation>(
      `${this.baseUrl}/conversations`,
      data,
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }

  getMessages(websiteToken: string, conversationId: number): Observable<Message[]> {
    return this.http.get<Message[]>(
      `${this.baseUrl}/conversations/${conversationId}/messages`,
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }

  sendMessage(websiteToken: string, conversationId: number, content: string): Observable<Message> {
    return this.http.post<Message>(
      `${this.baseUrl}/conversations/${conversationId}/messages`,
      { content },
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }

  submitCsat(websiteToken: string, conversationId: number, data: CsatData): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/conversations/${conversationId}/csat`,
      { rating: data.rating, feedback: data.feedback },
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }

  uploadAttachment(websiteToken: string, conversationId: number, file: File): Observable<Message> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<Message>(
      `${this.baseUrl}/conversations/${conversationId}/attachments`,
      formData,
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }

  getWidgetConfig(websiteToken: string): Observable<WidgetConfig> {
    return this.http.get<WidgetConfig>(
      `${this.baseUrl}/config`,
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }
}
