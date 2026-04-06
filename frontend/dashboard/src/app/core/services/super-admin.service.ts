import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { User } from '@core/models/user.model';

export interface Account {
  id: number;
  name: string;
  status: string;
  userCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface AdminStats {
  totalAccounts: number;
  totalUsers: number;
  activeConversations: number;
}

@Injectable({
  providedIn: 'root',
})
export class SuperAdminService {
  private readonly api = inject(ApiService);
  private readonly basePath = '/super_admin';

  getAccounts(): Observable<Account[]> {
    return this.api.get<Account[]>(`${this.basePath}/accounts`);
  }

  createAccount(name: string): Observable<{ id: number }> {
    return this.api.post<{ id: number }>(`${this.basePath}/accounts`, { name });
  }

  getUsers(): Observable<User[]> {
    return this.api.get<User[]>(`${this.basePath}/users`);
  }

  getConfig(): Observable<Record<string, string>> {
    return this.api.get<Record<string, string>>(`${this.basePath}/config`);
  }

  updateConfig(key: string, value: string): Observable<void> {
    return this.api.patch<void>(`${this.basePath}/config`, { key, value });
  }

  getStats(): Observable<AdminStats> {
    return this.api.get<AdminStats>(`${this.basePath}/stats`);
  }
}
