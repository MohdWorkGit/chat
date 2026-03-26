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
  private readonly baseUrl = '/api/v1/widget';

  constructor(private readonly http: HttpClient) {}

  createConversation(websiteToken: string, data: CreateConversationRequest): Observable<Conversation> {
    return this.http.post<Conversation>(
      `${this.baseUrl}/conversations`,
      data,
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }

  getConversation(websiteToken: string, conversationId: number): Observable<Conversation> {
    return this.http.get<Conversation>(
      `${this.baseUrl}/conversations/${conversationId}`,
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
      data,
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }

  sendTypingStatus(websiteToken: string, conversationId: number, typing: boolean): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/conversations/${conversationId}/typing`,
      { typing },
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }

  uploadAttachment(conversationId: number, file: File): Observable<Message> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<Message>(
      `${this.baseUrl}/conversations/${conversationId}/attachments`,
      formData,
    );
  }

  getWidgetConfig(websiteToken: string): Observable<WidgetConfig> {
    return this.http.get<WidgetConfig>(
      `${this.baseUrl}/config`,
      { headers: { 'X-Website-Token': websiteToken } },
    );
  }
}
