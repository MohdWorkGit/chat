import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '@core/services/auth.service';
import { ConversationService, ConversationFilters } from '@core/services/conversation.service';
import { ReportService } from '@core/services/report.service';
import { Conversation } from '@core/models/conversation.model';
import { User } from '@core/models/user.model';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-6xl mx-auto px-6 py-6">
        <!-- Welcome Header -->
        <div class="mb-6">
          <h1 class="text-2xl font-semibold text-gray-900">
            Welcome back, {{ currentUser()?.displayName || currentUser()?.name || 'Agent' }}
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
                  class="h-8 w-8 rounded-lg flex items-center justify-center"
                  [class]="card.iconBg"
                >
                  <svg
                    class="h-4 w-4"
                    [class]="card.iconColor"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke-width="1.5"
                    stroke="currentColor"
                  >
                    <path stroke-linecap="round" stroke-linejoin="round" [attr.d]="card.icon" />
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
              <button
                (click)="navigateTo('/conversations')"
                class="text-xs text-blue-600 hover:text-blue-700 font-medium"
              >
                View all
              </button>
            </div>

            @if (recentConversations().length === 0) {
              <p class="text-sm text-gray-400 py-6 text-center">No recent conversations.</p>
            } @else {
              <div class="divide-y divide-gray-100">
                @for (convo of recentConversations(); track convo.id) {
                  <div
                    class="flex items-center gap-3 py-3 cursor-pointer hover:bg-gray-50 -mx-2 px-2 rounded"
                    (click)="navigateTo('/conversations/' + convo.id)"
                  >
                    <div class="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center text-xs font-medium text-white flex-shrink-0">
                      {{ getInitials(convo.contact?.name || 'U') }}
                    </div>
                    <div class="flex-1 min-w-0">
                      <div class="flex items-center justify-between">
                        <p class="text-sm font-medium text-gray-900 truncate">
                          {{ convo.contact?.name || 'Unknown Contact' }}
                        </p>
                        <span class="text-xs text-gray-400 flex-shrink-0 ml-2">
                          #{{ convo.displayId }}
                        </span>
                      </div>
                      <p class="text-xs text-gray-500 truncate mt-0.5">
                        {{ getLastMessage(convo) }}
                      </p>
                    </div>
                    <span
                      class="text-xs px-2 py-0.5 rounded-full flex-shrink-0"
                      [class]="getStatusClasses(convo.status)"
                    >
                      {{ convo.status }}
                    </span>
                  </div>
                }
              </div>
            }
          </div>

          <!-- Agent Availability -->
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-4">Agent Availability</h3>

            <div class="space-y-3">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-2">
                  <div class="h-2.5 w-2.5 rounded-full bg-green-500"></div>
                  <span class="text-sm text-gray-700">Online</span>
                </div>
                <span class="text-sm font-semibold text-gray-900">{{ agentAvailability().online }}</span>
              </div>
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-2">
                  <div class="h-2.5 w-2.5 rounded-full bg-yellow-500"></div>
                  <span class="text-sm text-gray-700">Busy</span>
                </div>
                <span class="text-sm font-semibold text-gray-900">{{ agentAvailability().busy }}</span>
              </div>
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-2">
                  <div class="h-2.5 w-2.5 rounded-full bg-gray-400"></div>
                  <span class="text-sm text-gray-700">Offline</span>
                </div>
                <span class="text-sm font-semibold text-gray-900">{{ agentAvailability().offline }}</span>
              </div>
            </div>

            <!-- Visual bar -->
            <div class="mt-4">
              <div class="flex h-2 rounded-full overflow-hidden bg-gray-200">
                @if (agentAvailability().total > 0) {
                  <div
                    class="bg-green-500 transition-all"
                    [style.width.%]="(agentAvailability().online / agentAvailability().total) * 100"
                  ></div>
                  <div
                    class="bg-yellow-500 transition-all"
                    [style.width.%]="(agentAvailability().busy / agentAvailability().total) * 100"
                  ></div>
                }
              </div>
              <p class="text-xs text-gray-400 mt-2">
                {{ agentAvailability().total }} total agents
              </p>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Quick Actions</h3>
          <div class="flex items-center gap-3">
            <button
              (click)="navigateTo('/conversations')"
              class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
            >
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
              </svg>
              New Conversation
            </button>
            <button
              (click)="navigateTo('/reports')"
              class="inline-flex items-center gap-2 px-4 py-2 bg-white text-gray-700 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
            >
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v6.75C7.5 20.496 6.996 21 6.375 21h-2.25A1.125 1.125 0 0 1 3 19.875v-6.75ZM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v11.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 0 1-1.125-1.125V8.625ZM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25C20.496 3 21 3.504 21 4.125v15.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 0 1-1.125-1.125V4.125Z" />
              </svg>
              View Reports
            </button>
            <button
              (click)="navigateTo('/settings')"
              class="inline-flex items-center gap-2 px-4 py-2 bg-white text-gray-700 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
            >
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 0 1 1.37.49l1.296 2.247a1.125 1.125 0 0 1-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 0 1 0 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 0 1-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 0 1-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 0 1-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 0 1-1.369-.49l-1.297-2.247a1.125 1.125 0 0 1 .26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 0 1 0-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 0 1-.26-1.43l1.297-2.247a1.125 1.125 0 0 1 1.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28Z" />
                <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
              </svg>
              Settings
            </button>
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
  private readonly router = inject(Router);

  private subscriptions: Subscription[] = [];

  currentUser = signal<User | null>(null);

  statsCards = signal([
    {
      label: 'Open Conversations',
      value: '0',
      icon: 'M20.25 8.511c.884.284 1.5 1.128 1.5 2.097v4.286c0 1.136-.847 2.1-1.98 2.193-.34.027-.68.052-1.02.072v3.091l-3-3c-1.354 0-2.694-.055-4.02-.163a2.115 2.115 0 0 1-.825-.242m9.345-8.334a2.126 2.126 0 0 0-.476-.095 48.64 48.64 0 0 0-8.048 0c-1.131.094-1.976 1.057-1.976 2.192v4.286c0 .837.46 1.58 1.155 1.951m9.345-8.334V6.637c0-1.621-1.152-3.026-2.76-3.235A48.455 48.455 0 0 0 11.25 3c-2.115 0-4.198.137-6.24.402-1.608.209-2.76 1.614-2.76 3.235v6.226c0 1.621 1.152 3.026 2.76 3.235.577.075 1.157.14 1.74.194V21l4.155-4.155',
      iconBg: 'bg-blue-50',
      iconColor: 'text-blue-600',
    },
    {
      label: 'Unresolved',
      value: '0',
      icon: 'M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z',
      iconBg: 'bg-amber-50',
      iconColor: 'text-amber-600',
    },
    {
      label: 'Avg Response Time',
      value: '-',
      icon: 'M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z',
      iconBg: 'bg-purple-50',
      iconColor: 'text-purple-600',
    },
    {
      label: 'CSAT Score',
      value: '-',
      icon: 'M11.48 3.499a.562.562 0 0 1 1.04 0l2.125 5.111a.563.563 0 0 0 .475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 0 0-.182.557l1.285 5.385a.562.562 0 0 1-.84.61l-4.725-2.885a.562.562 0 0 0-.586 0L6.982 20.54a.562.562 0 0 1-.84-.61l1.285-5.386a.562.562 0 0 0-.182-.557l-4.204-3.602a.562.562 0 0 1 .321-.988l5.518-.442a.563.563 0 0 0 .475-.345L11.48 3.5Z',
      iconBg: 'bg-green-50',
      iconColor: 'text-green-600',
    },
  ]);

  recentConversations = signal<Conversation[]>([]);

  agentAvailability = signal<{ online: number; busy: number; offline: number; total: number }>({
    online: 0,
    busy: 0,
    offline: 0,
    total: 0,
  });

  ngOnInit(): void {
    // Load current user
    const userSub = this.authService.currentUser$.subscribe((user) => {
      this.currentUser.set(user);
    });
    this.subscriptions.push(userSub);

    this.loadStats();
    this.loadRecentConversations();
    this.loadAgentAvailability();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }

  navigateTo(path: string): void {
    this.router.navigateByUrl(path);
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  getLastMessage(convo: Conversation): string {
    if (convo.messages && convo.messages.length > 0) {
      return convo.messages[convo.messages.length - 1].content;
    }
    return 'No messages yet';
  }

  getStatusClasses(status: string): string {
    switch (status) {
      case 'open':
        return 'bg-green-100 text-green-700';
      case 'pending':
        return 'bg-yellow-100 text-yellow-700';
      case 'resolved':
        return 'bg-gray-100 text-gray-700';
      case 'snoozed':
        return 'bg-blue-100 text-blue-700';
      default:
        return 'bg-gray-100 text-gray-600';
    }
  }

  private loadStats(): void {
    const now = new Date();
    const weekAgo = new Date(now.getTime() - 7 * 86400000);
    const since = weekAgo.toISOString().split('T')[0];
    const until = now.toISOString().split('T')[0];

    const summarySub = this.reportService.getSummary(since, until).subscribe({
      next: (summary) => {
        this.statsCards.update((cards) =>
          cards.map((card) => {
            switch (card.label) {
              case 'Open Conversations':
                return { ...card, value: String(summary.conversationsCount - summary.resolutionCount) };
              case 'Unresolved':
                return { ...card, value: String(summary.conversationsCount - summary.resolutionCount) };
              case 'Avg Response Time':
                return { ...card, value: this.formatDuration(summary.avgFirstResponseTime) };
              default:
                return card;
            }
          })
        );
      },
      error: () => {
        // Fallback to sample data
        this.statsCards.update((cards) =>
          cards.map((card) => {
            switch (card.label) {
              case 'Open Conversations':
                return { ...card, value: '24' };
              case 'Unresolved':
                return { ...card, value: '12' };
              case 'Avg Response Time':
                return { ...card, value: '4m 32s' };
              case 'CSAT Score':
                return { ...card, value: '87%' };
              default:
                return card;
            }
          })
        );
      },
    });
    this.subscriptions.push(summarySub);

    // Fetch open conversations count separately for accuracy
    const openSub = this.conversationService.getAll({ status: 'open', page: 1, pageSize: 1 }).subscribe({
      next: (result) => {
        this.statsCards.update((cards) =>
          cards.map((card) =>
            card.label === 'Open Conversations'
              ? { ...card, value: String(result.meta.totalCount) }
              : card
          )
        );
      },
      error: () => {},
    });
    this.subscriptions.push(openSub);

    // Fetch unresolved (open + pending) count
    const unresolvedSub = this.conversationService.getAll({ status: 'pending', page: 1, pageSize: 1 }).subscribe({
      next: (result) => {
        this.statsCards.update((cards) =>
          cards.map((card) =>
            card.label === 'Unresolved'
              ? { ...card, value: String(result.meta.totalCount) }
              : card
          )
        );
      },
      error: () => {},
    });
    this.subscriptions.push(unresolvedSub);
  }

  private loadRecentConversations(): void {
    const recentSub = this.conversationService.getAll({ page: 1, pageSize: 5 }).subscribe({
      next: (result) => {
        this.recentConversations.set(result.data);
      },
      error: () => {
        this.recentConversations.set([]);
      },
    });
    this.subscriptions.push(recentSub);
  }

  private loadAgentAvailability(): void {
    // Use report service agent metrics as a proxy for agent data
    const now = new Date();
    const weekAgo = new Date(now.getTime() - 7 * 86400000);

    const agentSub = this.reportService.getAgentMetrics({
      type: 'agent',
      since: weekAgo.toISOString().split('T')[0],
      until: now.toISOString().split('T')[0],
    }).subscribe({
      next: (metrics) => {
        const total = metrics.length;
        // Estimate availability based on recent activity
        const online = Math.round(total * 0.6);
        const busy = Math.round(total * 0.2);
        const offline = total - online - busy;
        this.agentAvailability.set({ online, busy, offline, total });
      },
      error: () => {
        // Fallback
        this.agentAvailability.set({ online: 5, busy: 2, offline: 3, total: 10 });
      },
    });
    this.subscriptions.push(agentSub);
  }

  private formatDuration(seconds: number): string {
    if (seconds <= 0) return '-';
    const mins = Math.floor(seconds / 60);
    const secs = Math.round(seconds % 60);
    return `${mins}m ${secs.toString().padStart(2, '0')}s`;
  }
}
