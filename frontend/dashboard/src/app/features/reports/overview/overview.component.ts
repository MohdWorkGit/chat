import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-reports-overview',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-6xl mx-auto px-6 py-6">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <h1 class="text-xl font-semibold text-gray-900">Reports Overview</h1>

          <!-- Date Range Picker -->
          <div class="flex items-center gap-2">
            <div class="flex items-center gap-2 bg-white border border-gray-300 rounded-lg px-3 py-1.5">
              <label class="text-xs text-gray-500">From</label>
              <input
                type="date"
                [(ngModel)]="dateFrom"
                (change)="loadReport()"
                class="text-sm border-none focus:outline-none focus:ring-0 p-0"
              />
            </div>
            <span class="text-gray-400">-</span>
            <div class="flex items-center gap-2 bg-white border border-gray-300 rounded-lg px-3 py-1.5">
              <label class="text-xs text-gray-500">To</label>
              <input
                type="date"
                [(ngModel)]="dateTo"
                (change)="loadReport()"
                class="text-sm border-none focus:outline-none focus:ring-0 p-0"
              />
            </div>
          </div>
        </div>

        <!-- Summary Cards -->
        <div class="grid grid-cols-4 gap-4 mb-6">
          @for (card of summaryCards; track card.label) {
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <p class="text-sm text-gray-500 mb-1">{{ card.label }}</p>
              <p class="text-2xl font-bold text-gray-900">{{ card.value }}</p>
              <div class="flex items-center mt-2">
                @if (card.trend > 0) {
                  <span class="text-xs text-green-600 flex items-center gap-0.5">
                    <svg class="h-3 w-3" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 15.75 7.5-7.5 7.5 7.5" />
                    </svg>
                    {{ card.trend }}%
                  </span>
                } @else if (card.trend < 0) {
                  <span class="text-xs text-red-600 flex items-center gap-0.5">
                    <svg class="h-3 w-3" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" d="m19.5 8.25-7.5 7.5-7.5-7.5" />
                    </svg>
                    {{ card.trend * -1 }}%
                  </span>
                } @else {
                  <span class="text-xs text-gray-400">No change</span>
                }
                <span class="text-xs text-gray-400 ml-1">vs previous period</span>
              </div>
            </div>
          }
        </div>

        <!-- Bar Chart: Conversations by Day -->
        <div class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Conversations Over Time</h3>
          <div class="flex items-end gap-2 h-48">
            @for (bar of chartData; track bar.label) {
              <div class="flex-1 flex flex-col items-center gap-1">
                <span class="text-xs text-gray-500">{{ bar.value }}</span>
                <div
                  class="w-full rounded-t transition-all duration-300"
                  [class]="bar.color"
                  [style.height.%]="getBarHeight(bar.value)"
                ></div>
                <span class="text-xs text-gray-400 mt-1">{{ bar.label }}</span>
              </div>
            }
          </div>
        </div>

        <!-- Agent Performance Table -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Agent Performance</h3>
          <table class="w-full">
            <thead>
              <tr class="border-b border-gray-200">
                <th class="text-left text-xs font-medium text-gray-500 uppercase pb-3">Agent</th>
                <th class="text-left text-xs font-medium text-gray-500 uppercase pb-3">Conversations</th>
                <th class="text-left text-xs font-medium text-gray-500 uppercase pb-3">Resolved</th>
                <th class="text-left text-xs font-medium text-gray-500 uppercase pb-3">Avg Response Time</th>
                <th class="text-left text-xs font-medium text-gray-500 uppercase pb-3">CSAT</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              @for (agent of agentPerformance; track agent.name) {
                <tr>
                  <td class="py-3">
                    <div class="flex items-center gap-2">
                      <div class="h-7 w-7 rounded-full bg-gray-300 flex items-center justify-center text-xs font-medium text-white">
                        {{ getInitials(agent.name) }}
                      </div>
                      <span class="text-sm font-medium text-gray-900">{{ agent.name }}</span>
                    </div>
                  </td>
                  <td class="py-3 text-sm text-gray-700">{{ agent.conversations }}</td>
                  <td class="py-3 text-sm text-gray-700">{{ agent.resolved }}</td>
                  <td class="py-3 text-sm text-gray-700">{{ agent.avgResponseTime }}</td>
                  <td class="py-3">
                    <div class="flex items-center gap-2">
                      <div class="w-16 h-2 bg-gray-200 rounded-full overflow-hidden">
                        <div
                          class="h-full rounded-full transition-all"
                          [class]="agent.csat >= 80 ? 'bg-green-500' : agent.csat >= 60 ? 'bg-yellow-500' : 'bg-red-500'"
                          [style.width.%]="agent.csat"
                        ></div>
                      </div>
                      <span class="text-sm text-gray-700">{{ agent.csat }}%</span>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class ReportsOverviewComponent implements OnInit {
  dateFrom = '';
  dateTo = '';

  summaryCards = [
    { label: 'Total Conversations', value: '0', trend: 0 },
    { label: 'Resolved', value: '0', trend: 0 },
    { label: 'Avg Response Time', value: '-', trend: 0 },
    { label: 'CSAT Score', value: '-', trend: 0 },
  ];

  chartData: { label: string; value: number; color: string }[] = [];

  agentPerformance: {
    name: string;
    conversations: number;
    resolved: number;
    avgResponseTime: string;
    csat: number;
  }[] = [];

  private maxChartValue = 0;

  ngOnInit(): void {
    // Set default date range to last 7 days
    const now = new Date();
    const weekAgo = new Date(now.getTime() - 7 * 86400000);
    this.dateTo = this.formatDateInput(now);
    this.dateFrom = this.formatDateInput(weekAgo);
    this.loadReport();
  }

  loadReport(): void {
    // In a real app, this would call a ReportService or dispatch a store action.
    // Populating with sample data to demonstrate the component layout.
    this.summaryCards = [
      { label: 'Total Conversations', value: '247', trend: 12 },
      { label: 'Resolved', value: '189', trend: 8 },
      { label: 'Avg Response Time', value: '4m 32s', trend: -5 },
      { label: 'CSAT Score', value: '87%', trend: 3 },
    ];

    const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    const values = [32, 45, 28, 51, 40, 18, 33];
    this.maxChartValue = Math.max(...values);
    this.chartData = days.map((label, i) => ({
      label,
      value: values[i],
      color: 'bg-blue-500',
    }));

    this.agentPerformance = [
      { name: 'Alice Johnson', conversations: 68, resolved: 52, avgResponseTime: '3m 15s', csat: 92 },
      { name: 'Bob Williams', conversations: 55, resolved: 43, avgResponseTime: '5m 02s', csat: 85 },
      { name: 'Carol Davis', conversations: 62, resolved: 51, avgResponseTime: '4m 10s', csat: 88 },
      { name: 'David Martinez', conversations: 42, resolved: 30, avgResponseTime: '6m 45s', csat: 76 },
      { name: 'Eva Thompson', conversations: 20, resolved: 13, avgResponseTime: '8m 22s', csat: 70 },
    ];
  }

  getBarHeight(value: number): number {
    if (this.maxChartValue === 0) return 0;
    return Math.max((value / this.maxChartValue) * 100, 2);
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  private formatDateInput(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}
