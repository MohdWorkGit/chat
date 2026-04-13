import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Team, TeamMember } from '@core/models/team.model';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/teams');
  }

  getAll(): Observable<Team[]> {
    return this.api.get<Team[]>(this.basePath());
  }

  getById(id: number): Observable<Team> {
    return this.api.get<Team>(`${this.basePath()}/${id}`);
  }

  create(data: Partial<Team>): Observable<Team> {
    return this.api.post<Team>(this.basePath(), data);
  }

  update(id: number, data: Partial<Team>): Observable<Team> {
    return this.api.put<Team>(`${this.basePath()}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${id}`);
  }

  getMembers(teamId: number): Observable<TeamMember[]> {
    return this.api.get<TeamMember[]>(`${this.basePath()}/${teamId}/members`);
  }

  addMember(teamId: number, userId: number): Observable<TeamMember> {
    return this.api.post<TeamMember>(`${this.basePath()}/${teamId}/members`, { userId });
  }

  removeMember(teamId: number, userId: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath()}/${teamId}/members/${userId}`);
  }
}
