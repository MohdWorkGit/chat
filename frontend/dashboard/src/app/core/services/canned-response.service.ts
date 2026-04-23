import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CannedResponse } from '@core/models/canned-response.model';

@Injectable({
  providedIn: 'root',
})
export class CannedResponseService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/canned_responses');
  }

  getAll(): Observable<CannedResponse[]> {
    return this.api.get<CannedResponse[]>(this.basePath());
  }

  getById(id: number): Observable<CannedResponse> {
    return this.api.get<CannedResponse>(`${this.basePath()}/${id}`);
  }

  create(data: Partial<CannedResponse>): Observable<CannedResponse> {
    return this.api.post<CannedResponse>(this.basePath(), data);
  }

  update(id: number, data: Partial<CannedResponse>): Observable<CannedResponse> {
    return this.api.put<CannedResponse>(`${this.basePath()}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${id}`);
  }

  search(query: string): Observable<CannedResponse[]> {
    return this.api.get<CannedResponse[]>(`${this.basePath()}/search`, { q: query });
  }
}
