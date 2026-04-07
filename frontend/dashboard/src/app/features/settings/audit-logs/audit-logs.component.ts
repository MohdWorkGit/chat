import { Component, inject, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import {
  EnterpriseService,
  AuditLog,
  AuditLogFilter,
  PaginatedResult,
} from '@core/services/enterprise.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Audit Logs</h2>
          <p class="text-sm text-gray-500 mt-1">
            Activity trail for administrative actions taken in this account.
          </p>
        </div>
      </div>

      <!-- Filters -->
      <form
        [formGroup]="filterForm"
        (ngSubmit)="applyFilters()"
        class="bg-white rounded-lg border border-gray-200 p-4 mb-4 grid grid-cols-1 md:grid-cols-5 gap-3"
      >
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">User ID</label>
          <input
            type="number"
            formControlName="userId"
            class="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">Action</label>
          <input
            type="text"
            formControlName="action"
            placeholder="e.g. create, update"
            class="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">Resource Type</label>
          <input
            type="text"
            formControlName="auditableType"
            placeholder="e.g. Inbox, User"
            class="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">From</label>
          <input
            type="date"
            formControlName="dateFrom"
            class="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">To</label>
          <input
            type="date"
            formControlName="dateTo"
            class="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        <div class="md:col-span-5 flex justify-end gap-2">
          <button
            type="button"
            (click)="resetFilters()"
            class="px-4 py-2 text-sm font-medium text-gray-700 border border-gray-300 hover:bg-gray-50 rounded-lg transition-colors"
          >
            Reset
          </button>
          <button
            type="submit"
            class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
          >
            Apply Filters
          </button>
        </div>
      </form>

      <!-- Loading State -->
      @if (loading) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else if (error) {
        <div class="bg-red-50 border border-red-200 text-red-700 rounded-lg px-4 py-3 text-sm">
          {{ error }}
        </div>
      } @else if (logs.length === 0) {
        <div class="bg-white rounded-lg border border-gray-200 px-6 py-12 text-center">
          <p class="text-sm text-gray-500">No audit logs found for the selected filters.</p>
        </div>
      } @else {
        <!-- Logs Table -->
        <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">When</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Resource</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">IP</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Changes</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              @for (log of logs; track log.id) {
                <tr class="hover:bg-gray-50">
                  <td class="px-4 py-3 whitespace-nowrap text-xs text-gray-600">
                    {{ formatDate(log.createdAt) }}
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-900">
                    {{ log.userName || ('#' + log.userId) }}
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap">
                    <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium" [class]="actionBadgeClass(log.action)">
                      {{ log.action }}
                    </span>
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-700">
                    {{ log.auditableType }}#{{ log.auditableId }}
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap text-xs text-gray-500 font-mono">
                    {{ log.ipAddress || '-' }}
                  </td>
                  <td class="px-4 py-3 text-xs text-gray-500 max-w-md">
                    @if (log.changes) {
                      <button
                        type="button"
                        (click)="toggleChanges(log.id)"
                        class="text-blue-600 hover:text-blue-700"
                      >
                        {{ expandedId === log.id ? 'Hide' : 'View' }}
                      </button>
                      @if (expandedId === log.id) {
                        <pre class="mt-2 p-2 bg-gray-50 border border-gray-200 rounded text-xs text-gray-700 overflow-x-auto whitespace-pre-wrap break-all">{{ formatChanges(log.changes) }}</pre>
                      }
                    } @else {
                      <span>-</span>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="flex items-center justify-between mt-4">
          <p class="text-sm text-gray-500">
            Showing
            <span class="font-medium">{{ (page - 1) * pageSize + 1 }}</span>
            -
            <span class="font-medium">{{ Math.min(page * pageSize, totalCount) }}</span>
            of
            <span class="font-medium">{{ totalCount }}</span>
          </p>
          <div class="flex gap-2">
            <button
              type="button"
              (click)="prevPage()"
              [disabled]="page <= 1"
              class="px-3 py-1.5 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Previous
            </button>
            <button
              type="button"
              (click)="nextPage()"
              [disabled]="page * pageSize >= totalCount"
              class="px-3 py-1.5 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Next
            </button>
          </div>
        </div>
      }
    </div>
  `,
})
export class AuditLogsComponent implements OnInit {
  private readonly enterprise = inject(EnterpriseService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  protected readonly Math = Math;

  logs: AuditLog[] = [];
  loading = false;
  error: string | null = null;
  expandedId: number | null = null;

  page = 1;
  pageSize = 25;
  totalCount = 0;

  filterForm: FormGroup = this.fb.group({
    userId: [null],
    action: [''],
    auditableType: [''],
    dateFrom: [''],
    dateTo: [''],
  });

  ngOnInit(): void {
    this.fetch();
  }

  applyFilters(): void {
    this.page = 1;
    this.fetch();
  }

  resetFilters(): void {
    this.filterForm.reset({
      userId: null,
      action: '',
      auditableType: '',
      dateFrom: '',
      dateTo: '',
    });
    this.page = 1;
    this.fetch();
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page--;
      this.fetch();
    }
  }

  nextPage(): void {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.fetch();
    }
  }

  toggleChanges(id: number): void {
    this.expandedId = this.expandedId === id ? null : id;
  }

  formatDate(value: string): string {
    const d = new Date(value);
    return d.toLocaleString();
  }

  formatChanges(raw: string): string {
    try {
      return JSON.stringify(JSON.parse(raw), null, 2);
    } catch {
      return raw;
    }
  }

  actionBadgeClass(action: string): string {
    const a = action.toLowerCase();
    if (a.includes('create')) return 'bg-green-100 text-green-800';
    if (a.includes('delete')) return 'bg-red-100 text-red-800';
    if (a.includes('update')) return 'bg-blue-100 text-blue-800';
    return 'bg-gray-100 text-gray-800';
  }

  private fetch(): void {
    this.loading = true;
    this.error = null;
    this.cdr.markForCheck();

    const raw = this.filterForm.value;
    const filter: AuditLogFilter = {
      userId: raw.userId != null && raw.userId !== '' ? Number(raw.userId) : undefined,
      action: raw.action || undefined,
      auditableType: raw.auditableType || undefined,
      dateFrom: raw.dateFrom || undefined,
      dateTo: raw.dateTo || undefined,
      page: this.page,
      pageSize: this.pageSize,
    };

    this.enterprise.getAuditLogs(filter).subscribe({
      next: (result: PaginatedResult<AuditLog>) => {
        this.logs = result.items;
        this.totalCount = result.totalCount;
        this.page = result.page;
        this.pageSize = result.pageSize;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to load audit logs.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }
}
