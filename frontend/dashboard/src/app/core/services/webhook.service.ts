import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Webhook } from '@core/models/webhook.model';

@Injectable({
  providedIn: 'root',
})
export class WebhookService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/webhooks');
  }

  getAll(): Observable<Webhook[]> {
    return this.api.get<Webhook[]>(this.basePath());
  }

  getById(id: number): Observable<Webhook> {
    return this.api.get<Webhook>(`${this.basePath()}/${id}`);
  }

  create(data: Partial<Webhook>): Observable<Webhook> {
    return this.api.post<Webhook>(this.basePath(), data);
  }

  update(id: number, data: Partial<Webhook>): Observable<Webhook> {
    return this.api.put<Webhook>(`${this.basePath()}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${id}`);
  }
}
