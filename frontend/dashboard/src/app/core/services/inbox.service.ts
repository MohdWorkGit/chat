import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Inbox, InboxMember } from '@core/models/inbox.model';

@Injectable({
  providedIn: 'root',
})
export class InboxService {
  private readonly api = inject(ApiService);
  private readonly basePath = '/inboxes';

  getAll(): Observable<Inbox[]> {
    return this.api.get<Inbox[]>(this.basePath);
  }

  getById(id: number): Observable<Inbox> {
    return this.api.get<Inbox>(`${this.basePath}/${id}`);
  }

  create(data: Partial<Inbox>): Observable<Inbox> {
    return this.api.post<Inbox>(this.basePath, data);
  }

  update(id: number, data: Partial<Inbox>): Observable<Inbox> {
    return this.api.patch<Inbox>(`${this.basePath}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  getMembers(inboxId: number): Observable<InboxMember[]> {
    return this.api.get<InboxMember[]>(`${this.basePath}/${inboxId}/members`);
  }

  addMember(inboxId: number, userId: number): Observable<InboxMember> {
    return this.api.post<InboxMember>(`${this.basePath}/${inboxId}/members`, { userId });
  }

  removeMember(inboxId: number, userId: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${inboxId}/members/${userId}`);
  }
}
