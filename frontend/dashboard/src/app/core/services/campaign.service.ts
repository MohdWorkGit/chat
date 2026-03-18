import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Campaign } from '@core/models/campaign.model';

@Injectable({
  providedIn: 'root',
})
export class CampaignService {
  private readonly api = inject(ApiService);
  private readonly basePath = '/campaigns';

  getAll(): Observable<Campaign[]> {
    return this.api.get<Campaign[]>(this.basePath);
  }

  getById(id: number): Observable<Campaign> {
    return this.api.get<Campaign>(`${this.basePath}/${id}`);
  }

  create(data: Partial<Campaign>): Observable<Campaign> {
    return this.api.post<Campaign>(this.basePath, data);
  }

  update(id: number, data: Partial<Campaign>): Observable<Campaign> {
    return this.api.patch<Campaign>(`${this.basePath}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }
}
