import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { ReportService } from '@core/services/report.service';

@Component({
  selector: 'app-team-report',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Team Reports</h2>
        <form [formGroup]="filterForm" (ngSubmit)="loadReport()" class="flex items-center gap-3">
          <input
            type="date"
            formControlName="since"
            class="rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
          />
          <input
            type="date"
            formControlName="until"
            class="rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
          />
          <button
            type="submit"
            class="px-3 py-1.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
          >
            Apply
          </button>
        </form>
      </div>

      @if (loading) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else if (reportData) {
        <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Team</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Conversations</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Resolved</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Avg First Response</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Avg Resolution</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200">
              @for (team of reportData; track team.teamId) {
                <tr class="hover:bg-gray-50">
                  <td class="px-6 py-4 text-sm text-gray-900">{{ team.teamName }}</td>
                  <td class="px-6 py-4 text-sm text-gray-500">{{ team.totalConversations }}</td>
                  <td class="px-6 py-4 text-sm text-gray-500">{{ team.resolvedConversations }}</td>
                  <td class="px-6 py-4 text-sm text-gray-500">{{ team.avgFirstResponseTime }}</td>
                  <td class="px-6 py-4 text-sm text-gray-500">{{ team.avgResolutionTime }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="text-center py-12">
          <p class="text-sm text-gray-500">Select a date range to view team reports.</p>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class TeamReportComponent implements OnInit {
  private reportService = inject(ReportService);
  private fb = inject(FormBuilder);

  reportData: any[] | null = null;
  loading = false;

  filterForm: FormGroup = this.fb.group({
    since: [''],
    until: [''],
  });

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading = true;
    const params: Record<string, string> = { type: 'team' };
    const { since, until } = this.filterForm.value;
    if (since) params['since'] = since;
    if (until) params['until'] = until;

    this.reportService.getReport(params).subscribe({
      next: (data) => {
        this.reportData = data;
        this.loading = false;
      },
      error: () => {
        this.reportData = [];
        this.loading = false;
      },
    });
  }
}
