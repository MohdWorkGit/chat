import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { AutomationRule } from '@core/models/automation.model';

@Injectable({
  providedIn: 'root',
})
export class AutomationService {
  private readonly api = inject(ApiService);
  private readonly basePath = '/automation-rules';

  getAll(): Observable<AutomationRule[]> {
    return this.api.get<AutomationRule[]>(this.basePath);
  }

  getById(id: number): Observable<AutomationRule> {
    return this.api.get<AutomationRule>(`${this.basePath}/${id}`);
  }

  create(data: Partial<AutomationRule>): Observable<AutomationRule> {
    return this.api.post<AutomationRule>(this.basePath, data);
  }

  update(id: number, data: Partial<AutomationRule>): Observable<AutomationRule> {
    return this.api.patch<AutomationRule>(`${this.basePath}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }
}
