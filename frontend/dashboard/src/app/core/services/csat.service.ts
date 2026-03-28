import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CsatMetrics, CsatResponse } from '@core/models/csat.model';

@Injectable({
  providedIn: 'root',
})
export class CsatService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/csat_survey');
  }

  getResponses(params?: Record<string, string | number | boolean>): Observable<{ data: CsatResponse[]; total: number }> {
    return this.api.get<{ data: CsatResponse[]; total: number }>(this.basePath(), params);
  }

  getMetrics(params?: Record<string, string | number | boolean>): Observable<CsatMetrics> {
    return this.api.get<CsatMetrics>(`${this.basePath()}/metrics`, params);
  }
}
