import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Conversation, Message } from '@core/models/conversation.model';
import { PaginatedResult } from '@core/models/common.model';

export interface ConversationFilters {
  status?: string;
  assigneeId?: number;
  teamId?: number;
  inboxId?: number;
  labelId?: string;
  page?: number;
  pageSize?: number;
  query?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ConversationService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/conversations');
  }

  getAll(filters: ConversationFilters = {}): Observable<PaginatedResult<Conversation>> {
    return this.api.get<PaginatedResult<Conversation>>(this.basePath(), filters as Record<string, string | number | boolean>);
  }

  getById(id: number): Observable<Conversation> {
    return this.api.get<Conversation>(`${this.basePath()}/${id}`);
  }

  create(data: Partial<Conversation>): Observable<Conversation> {
    return this.api.post<Conversation>(this.basePath(), data);
  }

  update(id: number, data: Partial<Conversation>): Observable<Conversation> {
    return this.api.patch<Conversation>(`${this.basePath()}/${id}`, data);
  }

  updateStatus(id: number, status: string): Observable<Conversation> {
    return this.api.patch<Conversation>(`${this.basePath()}/${id}/status`, { status });
  }

  assign(id: number, assigneeId: number): Observable<Conversation> {
    return this.api.patch<Conversation>(`${this.basePath()}/${id}/assign`, { assigneeId });
  }

  assignTeam(id: number, teamId: number): Observable<Conversation> {
    return this.api.patch<Conversation>(`${this.basePath()}/${id}/assign`, { teamId });
  }

  addLabel(id: number, label: string): Observable<Conversation> {
    return this.api.post<Conversation>(`${this.basePath()}/${id}/labels`, { label });
  }

  removeLabel(id: number, label: string): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${id}/labels/${label}`);
  }

  getMessages(conversationId: number, page = 1): Observable<PaginatedResult<Message>> {
    return this.api.get<PaginatedResult<Message>>(
      this.api.accountPath(`/conversations/${conversationId}/messages`),
      { page }
    );
  }

  sendMessage(conversationId: number, content: string, isPrivate = false): Observable<Message> {
    return this.api.post<Message>(
      this.api.accountPath(`/conversations/${conversationId}/messages`),
      {
        content,
        private: isPrivate,
        messageType: 'outgoing',
      }
    );
  }

  sendAttachment(conversationId: number, formData: FormData): Observable<Message> {
    return this.api.upload<Message>(
      this.api.accountPath(`/conversations/${conversationId}/messages`),
      formData
    );
  }

  mute(id: number): Observable<void> {
    return this.api.post<void>(`${this.basePath()}/${id}/mute`);
  }

  unmute(id: number): Observable<void> {
    return this.api.post<void>(`${this.basePath()}/${id}/unmute`);
  }

  search(query: string, page = 1): Observable<PaginatedResult<Conversation>> {
    return this.api.get<PaginatedResult<Conversation>>(
      this.api.accountPath('/search'),
      { query, page }
    );
  }
}
