import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Contact } from '@core/models/contact.model';
import { PaginatedResult } from '@core/models/common.model';
import { Conversation } from '@core/models/conversation.model';

export interface ContactFilters {
  page?: number;
  pageSize?: number;
  query?: string;
  contactType?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/contacts');
  }

  getAll(filters: ContactFilters = {}): Observable<PaginatedResult<Contact>> {
    return this.api.get<PaginatedResult<Contact>>(this.basePath(), filters as Record<string, string | number | boolean>);
  }

  getById(id: number): Observable<Contact> {
    return this.api.get<Contact>(`${this.basePath()}/${id}`);
  }

  create(data: Partial<Contact>): Observable<Contact> {
    return this.api.post<Contact>(this.basePath(), data);
  }

  update(id: number, data: Partial<Contact>): Observable<Contact> {
    return this.api.put<Contact>(`${this.basePath()}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${id}`);
  }

  merge(baseContactId: number, mergeContactId: number): Observable<Contact> {
    return this.api.post<Contact>(`${this.basePath()}/${baseContactId}/merge`, { mergeContactId });
  }

  getConversations(contactId: number, page = 1): Observable<PaginatedResult<Conversation>> {
    return this.api.get<PaginatedResult<Conversation>>(`${this.basePath()}/${contactId}/conversations`, { page });
  }

  search(query: string, page = 1): Observable<PaginatedResult<Contact>> {
    return this.api.get<PaginatedResult<Contact>>(`${this.basePath()}/search`, { query, page });
  }

  addNote(contactId: number, content: string): Observable<{ id: number; content: string; createdAt: string }> {
    return this.api.post(`${this.basePath()}/${contactId}/notes`, { content });
  }

  getNotes(contactId: number): Observable<{ id: number; content: string; createdAt: string }[]> {
    return this.api.get(`${this.basePath()}/${contactId}/notes`);
  }
}
