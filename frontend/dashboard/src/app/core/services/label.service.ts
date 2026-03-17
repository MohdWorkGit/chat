import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Label } from '@core/models/label.model';

@Injectable({
  providedIn: 'root',
})
export class LabelService {
  private readonly api = inject(ApiService);
  private readonly basePath = '/labels';

  getAll(): Observable<Label[]> {
    return this.api.get<Label[]>(this.basePath);
  }

  getById(id: number): Observable<Label> {
    return this.api.get<Label>(`${this.basePath}/${id}`);
  }

  create(data: Partial<Label>): Observable<Label> {
    return this.api.post<Label>(this.basePath, data);
  }

  update(id: number, data: Partial<Label>): Observable<Label> {
    return this.api.patch<Label>(`${this.basePath}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }
}
