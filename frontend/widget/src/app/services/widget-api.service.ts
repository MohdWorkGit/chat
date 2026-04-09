import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EMPTY, Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
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
  private readonly baseUrl = '/api/v1/widget';

  constructor(private readonly http: HttpClient) {}

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

  // TODO: backend has no POST /widget/conversations/{id}/csat endpoint yet.
  // The request is still fired so that it starts working automatically as
  // soon as the backend adds the route, but any failure is swallowed so the
  // UI can close the CSAT form without showing an error.
  submitCsat(websiteToken: string, conversationId: number, data: CsatData): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/conversations/${conversationId}/csat`,
      data,
      { headers: { 'X-Website-Token': websiteToken } },
    ).pipe(
      catchError((err) => {
        console.warn('Widget CSAT submit failed (endpoint not implemented?):', err);
        return EMPTY;
      }),
    );
  }

  // TODO: backend has no POST /widget/conversations/{id}/attachments endpoint
  // yet. Same strategy as submitCsat — fire the request and silently ignore
  // failures until the backend catches up.
  uploadAttachment(websiteToken: string, conversationId: number, file: File): Observable<Message> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<Message>(
      `${this.baseUrl}/conversations/${conversationId}/attachments`,
      formData,
      { headers: { 'X-Website-Token': websiteToken } },
    ).pipe(
      catchError((err) => {
        console.warn('Widget attachment upload failed (endpoint not implemented?):', err);
        return EMPTY;
      }),
    );
  }

  getWidgetConfig(websiteToken: string): Observable<WidgetConfig> {
    return this.http.get<WidgetConfig>(
      `${this.baseUrl}/config`,
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }
}
