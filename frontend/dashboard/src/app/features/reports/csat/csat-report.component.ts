import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { CsatService } from '@core/services/csat.service';
import { CsatMetrics } from '@core/models/csat.model';

@Component({
  selector: 'app-csat-report',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">CSAT Reports</h2>
        <form [formGroup]="filterForm" (ngSubmit)="loadMetrics()" class="flex items-center gap-3">
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
      } @else if (metrics) {
        <!-- Summary Cards -->
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <div class="bg-white rounded-lg border border-gray-200 p-4">
            <p class="text-xs font-medium text-gray-500 uppercase">Total Responses</p>
            <p class="text-2xl font-bold text-gray-900 mt-1">{{ metrics.totalResponses }}</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-4">
            <p class="text-xs font-medium text-gray-500 uppercase">Average Rating</p>
            <p class="text-2xl font-bold text-gray-900 mt-1">{{ metrics.averageRating }}/5</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-4">
            <p class="text-xs font-medium text-gray-500 uppercase">Satisfaction Score</p>
            <p class="text-2xl font-bold text-green-600 mt-1">{{ metrics.satisfactionScore }}%</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-4">
            <p class="text-xs font-medium text-gray-500 uppercase">Response Rate</p>
            <p class="text-2xl font-bold text-gray-900 mt-1">--</p>
          </div>
        </div>

        <!-- Rating Distribution -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <h3 class="text-sm font-medium text-gray-900 mb-4">Rating Distribution</h3>
          <div class="space-y-3">
            @for (rating of [5, 4, 3, 2, 1]; track rating) {
              <div class="flex items-center gap-3">
                <span class="text-sm text-gray-600 w-16">{{ rating }} star{{ rating !== 1 ? 's' : '' }}</span>
                <div class="flex-1 bg-gray-100 rounded-full h-4 overflow-hidden">
                  <div
                    class="h-4 rounded-full transition-all"
                    [class]="rating >= 4 ? 'bg-green-500' : rating === 3 ? 'bg-yellow-500' : 'bg-red-500'"
                    [style.width.%]="getDistributionPercent(rating)"
                  ></div>
                </div>
                <span class="text-sm text-gray-500 w-10 text-right">
                  {{ metrics.ratingDistribution[rating] || 0 }}
                </span>
              </div>
            }
          </div>
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
export class CsatReportComponent implements OnInit {
  private csatService = inject(CsatService);
  private fb = inject(FormBuilder);

  metrics: CsatMetrics | null = null;
  loading = false;

  filterForm: FormGroup = this.fb.group({
    since: [''],
    until: [''],
  });

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.loading = true;
    const params: Record<string, string> = {};
    const { since, until } = this.filterForm.value;
    if (since) params['since'] = since;
    if (until) params['until'] = until;

    this.csatService.getMetrics(params).subscribe({
      next: (metrics) => {
        this.metrics = metrics;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  getDistributionPercent(rating: number): number {
    if (!this.metrics || this.metrics.totalResponses === 0) return 0;
    return ((this.metrics.ratingDistribution[rating] || 0) / this.metrics.totalResponses) * 100;
  }
}
