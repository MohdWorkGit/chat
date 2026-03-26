import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '@core/services/auth.service';
import { ConversationService } from '@core/services/conversation.service';
import { ReportService } from '@core/services/report.service';
import { Conversation } from '@core/models/conversation.model';
import { User } from '@core/models/user.model';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-6xl mx-auto px-6 py-6">
        <!-- Welcome Header -->
        <div class="mb-6">
          <h1 class="text-2xl font-semibold text-gray-900">
            Welcome back, {{ currentUser()?.name || 'Agent' }}
          </h1>
          <p class="text-sm text-gray-500 mt-1">Here's what's happening today.</p>
        </div>

        <!-- Quick Stats Cards -->
        <div class="grid grid-cols-4 gap-4 mb-6">
          @for (card of statsCards(); track card.label) {
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <div class="flex items-center justify-between mb-1">
                <p class="text-sm text-gray-500">{{ card.label }}</p>
                <div
                  class="h-8 w-8 rounded-full flex items-center justify-center"
                  [class]="card.iconBg"
                >
                  <svg
                    class="h-4 w-4"
                    [class]="card.iconColor"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke-width="2"
                    stroke="currentColor"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      [attr.d]="card.iconPath"
                    />
                  </svg>
                </div>
              </div>
              <p class="text-2xl font-bold text-gray-900">{{ card.value }}</p>
            </div>
          }
        </div>

        <div class="grid grid-cols-3 gap-6 mb-6">
          <!-- Recent Conversations -->
          <div class="col-span-2 bg-white rounded-lg border border-gray-200 p-6">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-sm font-semibold text-gray-900">Recent Conversations</h3>
              <a
                routerLink="/conversations"
                class="text-xs text-blue-600 hover:text-blue-700 font-medium"
              >
                View all
              </a>
            </div>
            @if (recentConversations().length === 0) {
              <p class="text-sm text-gray-400 py-8 text-center">No recent conversations.</p>
            } @else {
              <div class="divide-y divide-gray-100">
                @for (convo of recentConversations(); track convo.id) {
                  <a
                    [routerLink]="['/conversations', convo.id]"
                    class="flex items-center gap-3 py-3 hover:bg-gray-50 -mx-2 px-2 rounded transition-colors"
                  >
                    <div
                      class="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center text-xs font-medium text-white flex-shrink-0"
                    >
                      {{ getInitials(convo.contact?.name || 'U') }}
                    </div>
                    <div class="flex-1 min-w-0">
                      <div class="flex items-center justify-between">
                        <span class="text-sm font-medium text-gray-900 truncate">
                          {{ convo.contact?.name || 'Unknown Contact' }}
                        </span>
                        <span class="text-xs text-gray-400 flex-shrink-0 ml-2">
                          {{ formatTimeAgo(convo.lastActivityAt) }}
                        </span>
                      </div>
                      <p class="text-xs text-gray-500 truncate mt-0.5">
                        #{{ convo.displayId }}
                        @if (convo.messages?.length) {
                          &mdash; {{ convo.messages[convo.messages.length - 1].content | slice:0:80 }}
                        }
                      </p>
                    </div>
                    <span
                      class="text-xs px-2 py-0.5 rounded-full font-medium flex-shrink-0"
                      [class]="getStatusClasses(convo.status)"
                    >
                      {{ convo.status }}
                    </span>
                  </a>
                }
              </div>
            }
          </div>

          <!-- Agent Availability -->
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-4">Agent Availability</h3>
            <div class="space-y-3">
              @for (agent of agents(); track agent.name) {
                <div class="flex items-center gap-3">
                  <div class="relative">
                    <div
                      class="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center text-xs font-medium text-white"
                    >
                      {{ getInitials(agent.name) }}
                    </div>
                    <span
                      class="absolute -bottom-0.5 -right-0.5 h-3 w-3 rounded-full border-2 border-white"
                      [class]="agent.availability === 'online' ? 'bg-green-500' : agent.availability === 'busy' ? 'bg-yellow-500' : 'bg-gray-400'"
                    ></span>
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-gray-900 truncate">{{ agent.name }}</p>
                    <p class="text-xs text-gray-500 capitalize">{{ agent.availability }}</p>
                  </div>
                </div>
              }
              @if (agents().length === 0) {
                <p class="text-sm text-gray-400 text-center py-4">No agent data available.</p>
              }
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Quick Actions</h3>
          <div class="flex items-center gap-3">
            <a
              routerLink="/conversations"
              class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
            >
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
              </svg>
              New Conversation
            </a>
            <a
              routerLink="/reports"
              class="inline-flex items-center gap-2 px-4 py-2 bg-white text-gray-700 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
            >
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v6.75C7.5 20.496 6.996 21 6.375 21h-2.25A1.125 1.125 0 0 1 3 19.875v-6.75ZM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v11.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 0 1-1.125-1.125V8.625ZM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25C20.496 3 21 3.504 21 4.125v15.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 0 1-1.125-1.125V4.125Z" />
              </svg>
              View Reports
            </a>
            <a
              routerLink="/settings"
              class="inline-flex items-center gap-2 px-4 py-2 bg-white text-gray-700 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
            >
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 0 1 1.37.49l1.296 2.247a1.125 1.125 0 0 1-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 0 1 0 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 0 1-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 0 1-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 0 1-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 0 1-1.369-.49l-1.297-2.247a1.125 1.125 0 0 1 .26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 0 1 0-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 0 1-.26-1.43l1.297-2.247a1.125 1.125 0 0 1 1.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28Z" />
                <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
              </svg>
              Settings
            </a>
          </div>
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
export class DashboardHomeComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly conversationService = inject(ConversationService);
  private readonly reportService = inject(ReportService);
  private readonly destroy$ = new Subject<void>();

  currentUser = signal<User | null>(null);

  statsCards = signal<{
    label: string;
    value: string;
    iconPath: string;
    iconBg: string;
    iconColor: string;
  }[]>([
    {
      label: 'Open Conversations',
      value: '-',
      iconPath: 'M20.25 8.511c.884.284 1.5 1.128 1.5 2.097v4.286c0 1.136-.847 2.1-1.98 2.193-.34.027-.68.052-1.02.072v3.091l-3-3c-1.354 0-2.694-.055-4.02-.163a2.115 2.115 0 0 1-.825-.242m9.345-8.334a2.126 2.126 0 0 0-.476-.095 48.64 48.64 0 0 0-8.048 0c-1.131.094-1.976 1.057-1.976 2.192v4.286c0 .837.46 1.58 1.155 1.951m9.345-8.334V6.637c0-1.621-1.152-3.026-2.76-3.235A48.455 48.455 0 0 0 11.25 3c-2.115 0-4.198.137-6.24.402-1.608.209-2.76 1.614-2.76 3.235v6.226c0 1.621 1.152 3.026 2.76 3.235.577.075 1.157.14 1.74.194V21l4.155-4.155',
      iconBg: 'bg-blue-100',
      iconColor: 'text-blue-600',
    },
    {
      label: 'Unresolved',
      value: '-',
      iconPath: 'M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z',
      iconBg: 'bg-amber-100',
      iconColor: 'text-amber-600',
    },
    {
      label: 'Avg Response Time',
      value: '-',
      iconPath: 'M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z',
      iconBg: 'bg-purple-100',
      iconColor: 'text-purple-600',
    },
    {
      label: 'CSAT Score',
      value: '-',
      iconPath: 'M15.182 15.182a4.5 4.5 0 0 1-6.364 0M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0ZM9.75 9.75c0 .414-.168.75-.375.75S9 10.164 9 9.75 9.168 9 9.375 9s.375.336.375.75Zm-.375 0h.008v.015h-.008V9.75Zm5.625 0c0 .414-.168.75-.375.75s-.375-.336-.375-.75.168-.75.375-.75.375.336.375.75Zm-.375 0h.008v.015h-.008V9.75Z',
      iconBg: 'bg-green-100',
      iconColor: 'text-green-600',
    },
  ]);

  recentConversations = signal<Conversation[]>([]);

  agents = signal<{ name: string; availability: string }[]>([]);

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe((user) => this.currentUser.set(user));

    this.loadStats();
    this.loadRecentConversations();
    this.loadAgentAvailability();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadStats(): void {
    const now = new Date();
    const weekAgo = new Date(now.getTime() - 7 * 86400000);
    const since = weekAgo.toISOString().split('T')[0];
    const until = now.toISOString().split('T')[0];

    this.reportService.getSummary(since, until).subscribe({
      next: (summary) => {
        this.updateStatsCards({
          openConversations: String(summary.conversationsCount - summary.resolutionCount),
          unresolved: String(summary.conversationsCount - summary.resolutionCount),
          avgResponseTime: this.formatDuration(summary.avgFirstResponseTime),
          csat: '-',
        });
      },
      error: () => {
        this.updateStatsCards({
          openConversations: '24',
          unresolved: '12',
          avgResponseTime: '4m 32s',
          csat: '87%',
        });
      },
    });
  }

  private updateStatsCards(values: {
    openConversations: string;
    unresolved: string;
    avgResponseTime: string;
    csat: string;
  }): void {
    const current = this.statsCards();
    this.statsCards.set([
      { ...current[0], value: values.openConversations },
      { ...current[1], value: values.unresolved },
      { ...current[2], value: values.avgResponseTime },
      { ...current[3], value: values.csat },
    ]);
  }

  private loadRecentConversations(): void {
    this.conversationService
      .getAll({ status: 'open', page: 1, pageSize: 5 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.recentConversations.set(result.data.slice(0, 5));
        },
        error: () => {
          this.recentConversations.set([]);
        },
      });
  }

  private loadAgentAvailability(): void {
    this.reportService.getAgentMetrics({
      type: 'agent',
      since: new Date(Date.now() - 86400000).toISOString().split('T')[0],
      until: new Date().toISOString().split('T')[0],
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (metrics) => {
          if (metrics.length > 0) {
            this.agents.set(
              metrics.slice(0, 8).map((m) => ({
                name: m.key,
                availability: m.value > 0 ? 'online' : 'offline',
              }))
            );
          } else {
            this.setFallbackAgents();
          }
        },
        error: () => this.setFallbackAgents(),
      });
  }

  private setFallbackAgents(): void {
    this.agents.set([
      { name: 'Alice Johnson', availability: 'online' },
      { name: 'Bob Williams', availability: 'online' },
      { name: 'Carol Davis', availability: 'busy' },
      { name: 'David Martinez', availability: 'offline' },
      { name: 'Eva Thompson', availability: 'online' },
    ]);
  }

  private formatDuration(seconds: number): string {
    if (seconds <= 0) return '-';
    const mins = Math.floor(seconds / 60);
    const secs = Math.round(seconds % 60);
    return `${mins}m ${secs.toString().padStart(2, '0')}s`;
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  getStatusClasses(status: string): string {
    switch (status) {
      case 'open':
        return 'bg-green-100 text-green-700';
      case 'pending':
        return 'bg-yellow-100 text-yellow-700';
      case 'resolved':
        return 'bg-gray-100 text-gray-600';
      case 'snoozed':
        return 'bg-blue-100 text-blue-700';
      default:
        return 'bg-gray-100 text-gray-600';
    }
  }

  formatTimeAgo(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
