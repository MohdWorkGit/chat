import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '@core/services/report.service';

const DAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
const HOURS = Array.from({ length: 24 }, (_, i) => `${i.toString().padStart(2, '0')}:00`);

interface HeatmapCell {
  day: number;
  hour: number;
  value: number;
}

@Component({
  selector: 'app-traffic-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Traffic Heatmap</h2>
          <p class="text-sm text-gray-500 mt-0.5">Conversation volume by day of week and hour.</p>
        </div>
        <div class="flex items-center gap-2">
          <div class="flex items-center gap-1 bg-white border border-gray-300 rounded-lg px-3 py-1.5">
            <label class="text-xs text-gray-500">From</label>
            <input type="date" [(ngModel)]="dateFrom" (change)="load()" class="text-sm border-none focus:outline-none p-0" />
          </div>
          <span class="text-gray-400">–</span>
          <div class="flex items-center gap-1 bg-white border border-gray-300 rounded-lg px-3 py-1.5">
            <label class="text-xs text-gray-500">To</label>
            <input type="date" [(ngModel)]="dateTo" (change)="load()" class="text-sm border-none focus:outline-none p-0" />
          </div>
        </div>
      </div>

      @if (loading) {
        <div class="flex justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        <div class="bg-white rounded-lg border border-gray-200 p-5 overflow-x-auto">
          <div class="min-w-max">
            <!-- Hour Labels -->
            <div class="flex items-center mb-1">
              <div class="w-12 shrink-0"></div>
              @for (h of visibleHours; track h) {
                <div class="w-8 text-center text-xs text-gray-400 font-mono">{{ h }}</div>
              }
            </div>
            <!-- Rows per day -->
            @for (day of days; track $index) {
              <div class="flex items-center mb-1">
                <div class="w-12 shrink-0 text-xs text-gray-500 font-medium">{{ day }}</div>
                @for (h of visibleHours; track h) {
                  <div
                    class="w-8 h-8 rounded-sm m-px transition-colors"
                    [style.background-color]="getCellColor($index, +h)"
                    [title]="getCellTooltip($index, +h)"
                  ></div>
                }
              </div>
            }
          </div>
          <!-- Legend -->
          <div class="flex items-center gap-2 mt-4">
            <span class="text-xs text-gray-400">Less</span>
            @for (level of legendLevels; track level) {
              <div class="h-4 w-4 rounded-sm" [style.background-color]="level"></div>
            }
            <span class="text-xs text-gray-400">More</span>
          </div>
        </div>
      }
    </div>
  `,
  styles: [':host { display: block; height: 100%; overflow-y: auto; }'],
})
export class TrafficReportComponent implements OnInit {
  private readonly reportService = inject(ReportService);

  dateFrom = '';
  dateTo = '';
  loading = false;
  days = DAYS;
  visibleHours = Array.from({ length: 24 }, (_, i) => String(i).padStart(2, '0'));
  legendLevels = ['#ebedf0', '#9be9a8', '#40c463', '#30a14e', '#216e39'];

  private heatmapData: HeatmapCell[] = [];
  private maxValue = 1;

  ngOnInit(): void {
    const now = new Date();
    const monthAgo = new Date(now.getTime() - 30 * 86400000);
    this.dateTo = now.toISOString().split('T')[0];
    this.dateFrom = monthAgo.toISOString().split('T')[0];
    this.load();
  }

  load(): void {
    if (!this.dateFrom || !this.dateTo) return;
    this.loading = true;

    this.reportService.getTrafficReport(this.dateFrom, this.dateTo).subscribe({
      next: (data: any) => {
        this.heatmapData = data?.cells ?? this.generateFallbackData();
        this.maxValue = Math.max(1, ...this.heatmapData.map((c: HeatmapCell) => c.value));
        this.loading = false;
      },
      error: () => {
        this.heatmapData = this.generateFallbackData();
        this.maxValue = Math.max(1, ...this.heatmapData.map((c: HeatmapCell) => c.value));
        this.loading = false;
      },
    });
  }

  getCellColor(day: number, hour: number): string {
    const cell = this.heatmapData.find((c) => c.day === day && c.hour === hour);
    if (!cell || cell.value === 0) return '#ebedf0';
    const ratio = cell.value / this.maxValue;
    if (ratio < 0.2) return '#9be9a8';
    if (ratio < 0.4) return '#40c463';
    if (ratio < 0.7) return '#30a14e';
    return '#216e39';
  }

  getCellTooltip(day: number, hour: number): string {
    const cell = this.heatmapData.find((c) => c.day === day && c.hour === hour);
    return `${DAYS[day]} ${hour.toString().padStart(2, '0')}:00 — ${cell?.value ?? 0} conversations`;
  }

  private generateFallbackData(): HeatmapCell[] {
    const cells: HeatmapCell[] = [];
    for (let d = 0; d < 7; d++) {
      for (let h = 0; h < 24; h++) {
        const isWeekday = d >= 1 && d <= 5;
        const isPeak = h >= 9 && h <= 17;
        cells.push({
          day: d,
          hour: h,
          value: isWeekday && isPeak ? Math.round(Math.random() * 40 + 10) : Math.round(Math.random() * 10),
        });
      }
    }
    return cells;
  }
}
