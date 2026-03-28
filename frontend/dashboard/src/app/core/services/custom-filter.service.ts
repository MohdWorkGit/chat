import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CustomFilter } from '@core/models/custom-filter.model';

@Injectable({
  providedIn: 'root',
})
export class CustomFilterService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/custom_filters');
  }

  getAll(): Observable<CustomFilter[]> {
    return this.api.get<CustomFilter[]>(this.basePath());
  }

  create(data: Partial<CustomFilter>): Observable<CustomFilter> {
    return this.api.post<CustomFilter>(this.basePath(), data);
  }

  update(id: number, data: Partial<CustomFilter>): Observable<CustomFilter> {
    return this.api.patch<CustomFilter>(`${this.basePath()}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${id}`);
  }
}
