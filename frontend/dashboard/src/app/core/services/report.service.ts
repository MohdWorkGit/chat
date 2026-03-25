import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Report, ReportMetric } from '@core/models/report.model';

export interface ReportFilters {
  type: 'account' | 'agent' | 'inbox' | 'team' | 'label';
  since: string;
  until: string;
  groupBy?: 'day' | 'week' | 'month' | 'year';
  id?: number;
}

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private readonly api = inject(ApiService);
  private basePath(): string {
    return this.api.accountPath('/reports');
  }

  getOverview(filters: ReportFilters): Observable<Report> {
    return this.api.get<Report>(`${this.basePath()}/overview`, filters as unknown as Record<string, string | number | boolean>);
  }

  getConversationMetrics(filters: ReportFilters): Observable<ReportMetric[]> {
    return this.api.get<ReportMetric[]>(`${this.basePath()}/conversations`, filters as unknown as Record<string, string | number | boolean>);
  }

  getAgentMetrics(filters: ReportFilters): Observable<ReportMetric[]> {
    return this.api.get<ReportMetric[]>(`${this.basePath()}/agents`, filters as unknown as Record<string, string | number | boolean>);
  }

  getInboxMetrics(filters: ReportFilters): Observable<ReportMetric[]> {
    return this.api.get<ReportMetric[]>(`${this.basePath()}/inboxes`, filters as unknown as Record<string, string | number | boolean>);
  }

  getTeamMetrics(filters: ReportFilters): Observable<ReportMetric[]> {
    return this.api.get<ReportMetric[]>(`${this.basePath()}/teams`, filters as unknown as Record<string, string | number | boolean>);
  }

  getSummary(since: string, until: string): Observable<{
    conversationsCount: number;
    incomingMessagesCount: number;
    outgoingMessagesCount: number;
    avgFirstResponseTime: number;
    avgResolutionTime: number;
    resolutionCount: number;
  }> {
    return this.api.get(`${this.basePath()}/summary`, { since, until });
  }
}
