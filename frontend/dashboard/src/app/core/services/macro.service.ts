import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Macro } from '@core/models/macro.model';

@Injectable({
  providedIn: 'root',
})
export class MacroService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/macros');
  }

  getAll(): Observable<Macro[]> {
    return this.api.get<Macro[]>(this.basePath());
  }

  getById(id: number): Observable<Macro> {
    return this.api.get<Macro>(`${this.basePath()}/${id}`);
  }

  create(data: Partial<Macro>): Observable<Macro> {
    return this.api.post<Macro>(this.basePath(), data);
  }

  update(id: number, data: Partial<Macro>): Observable<Macro> {
    return this.api.put<Macro>(`${this.basePath()}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${id}`);
  }

  execute(id: number, conversationId: number): Observable<void> {
    return this.api.post<void>(`${this.basePath()}/${id}/execute`, { conversationId });
  }
}
