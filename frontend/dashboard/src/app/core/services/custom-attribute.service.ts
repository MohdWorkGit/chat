import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CustomAttribute } from '@core/models/custom-attribute.model';

@Injectable({
  providedIn: 'root',
})
export class CustomAttributeService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/custom-attributes');
  }

  getAll(): Observable<CustomAttribute[]> {
    return this.api.get<CustomAttribute[]>(this.basePath());
  }

  getById(id: number): Observable<CustomAttribute> {
    return this.api.get<CustomAttribute>(`${this.basePath()}/${id}`);
  }

  create(data: Partial<CustomAttribute>): Observable<CustomAttribute> {
    return this.api.post<CustomAttribute>(this.basePath(), data);
  }

  update(id: number, data: Partial<CustomAttribute>): Observable<CustomAttribute> {
    return this.api.patch<CustomAttribute>(`${this.basePath()}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${id}`);
  }
}
