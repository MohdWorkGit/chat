import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { ConversationsActions } from '@app/store/conversations/conversations.actions';
import {
  selectAllConversations,
  selectConversationsLoading,
  selectConversationFilters,
  selectConversationsPagination,
  selectSelectedConversationId,
} from '@app/store/conversations/conversations.selectors';
import { ConversationStatus, ConversationPriority, Conversation } from '@core/models/conversation.model';

@Component({
  selector: 'app-conversation-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex flex-col h-full border-r border-gray-200 bg-white">
      <!-- Search -->
      <div class="p-3 border-b border-gray-200">
        <div class="relative">
          <svg class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
          </svg>
          <input
            type="text"
            [(ngModel)]="searchQuery"
            (input)="onSearch()"
            placeholder="Search conversations..."
            class="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
      </div>

      <!-- Filter Tabs -->
      <div class="flex border-b border-gray-200">
        @for (tab of ownerTabs; track tab.value) {
          <button
            (click)="setOwnerFilter(tab.value)"
            class="flex-1 py-2.5 text-xs font-medium text-center transition-colors"
            [class]="activeOwnerTab === tab.value
              ? 'text-blue-600 border-b-2 border-blue-600'
              : 'text-gray-500 hover:text-gray-700'"
          >
            {{ tab.label }}
          </button>
        }
      </div>

      <!-- Status Filter -->
      <div class="px-3 py-2 border-b border-gray-200">
        <select
          [(ngModel)]="activeStatus"
          (ngModelChange)="onStatusChange($event)"
          class="w-full text-sm border border-gray-300 rounded-md px-2 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500"
        >
          <option value="">All Statuses</option>
          @for (status of statuses; track status) {
            <option [value]="status">{{ status | titlecase }}</option>
          }
        </select>
      </div>

      <!-- Conversation List -->
      <div class="flex-1 overflow-y-auto">
        @if (loading$ | async) {
          <div class="flex items-center justify-center py-8">
            <svg class="animate-spin h-6 w-6 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
            </svg>
          </div>
        }

        @for (conversation of conversations$ | async; track conversation.id) {
          <div
            (click)="selectConversation(conversation.id)"
            class="flex items-start gap-3 px-3 py-3 border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors"
            [class.bg-blue-50]="(selectedId$ | async) === conversation.id"
          >
            <!-- Avatar -->
            <div class="flex-shrink-0">
              @if (conversation.contact?.avatar) {
                <img [src]="conversation.contact!.avatar" [alt]="conversation.contact!.name" class="h-10 w-10 rounded-full object-cover" />
              } @else {
                <div class="h-10 w-10 rounded-full bg-gray-300 flex items-center justify-center text-sm font-medium text-white">
                  {{ getInitials(conversation.contact?.name || 'U') }}
                </div>
              }
            </div>

            <!-- Content -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between">
                <h4 class="text-sm font-medium text-gray-900 truncate">
                  {{ conversation.contact?.name || conversation.contact?.email || 'Unknown' }}
                </h4>
                <span class="text-xs text-gray-500 flex-shrink-0 ml-2">
                  {{ formatTime(conversation.lastActivityAt) }}
                </span>
              </div>
              <p class="text-xs text-gray-500 truncate mt-0.5">
                {{ getLastMessagePreview(conversation) }}
              </p>
              <div class="flex items-center gap-2 mt-1">
                <!-- Status Badge -->
                <span
                  class="inline-flex items-center px-1.5 py-0.5 rounded text-xs font-medium"
                  [class]="getStatusClasses(conversation.status)"
                >
                  {{ conversation.status }}
                </span>

                <!-- Priority Indicator -->
                @if (conversation.priority !== 'none') {
                  <span
                    class="inline-flex items-center text-xs"
                    [class]="getPriorityColor(conversation.priority)"
                  >
                    @switch (conversation.priority) {
                      @case ('urgent') { !! }
                      @case ('high') { ! }
                      @default { - }
                    }
                    {{ conversation.priority }}
                  </span>
                }

                <!-- Assignee -->
                @if (conversation.assignee) {
                  <span class="text-xs text-gray-400 ml-auto flex items-center gap-1">
                    @if (conversation.assignee.avatar) {
                      <img [src]="conversation.assignee.avatar" class="h-4 w-4 rounded-full" />
                    }
                    {{ conversation.assignee.name }}
                  </span>
                }

                <!-- Unread count -->
                @if (conversation.unreadCount > 0) {
                  <span class="ml-auto bg-blue-600 text-white text-xs rounded-full h-5 min-w-5 flex items-center justify-center px-1">
                    {{ conversation.unreadCount }}
                  </span>
                }
              </div>
            </div>
          </div>
        } @empty {
          @if (!(loading$ | async)) {
            <div class="flex flex-col items-center justify-center py-12 text-gray-400">
              <svg class="h-12 w-12 mb-3" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M20.25 8.511c.884.284 1.5 1.128 1.5 2.097v4.286c0 1.136-.847 2.1-1.98 2.193-.34.027-.68.052-1.02.072v3.091l-3-3c-1.354 0-2.694-.055-4.02-.163a2.115 2.115 0 0 1-.825-.242m9.345-8.334a2.126 2.126 0 0 0-.476-.095 48.64 48.64 0 0 0-8.048 0c-1.131.094-1.976 1.057-1.976 2.192v4.286c0 .837.46 1.58 1.155 1.951m9.345-8.334V6.637c0-1.621-1.152-3.026-2.76-3.235A48.455 48.455 0 0 0 11.25 3c-2.115 0-4.198.137-6.24.402-1.608.209-2.76 1.614-2.76 3.235v6.226c0 1.621 1.152 3.026 2.76 3.235.577.075 1.157.14 1.74.194V21l4.155-4.155" />
              </svg>
              <p class="text-sm">No conversations found</p>
            </div>
          }
        }
      </div>

      <!-- Pagination -->
      @if (pagination$ | async; as pagination) {
        @if (pagination.totalPages > 1) {
          <div class="flex items-center justify-between px-3 py-2 border-t border-gray-200 bg-gray-50">
            <button
              (click)="goToPage(pagination.currentPage - 1)"
              [disabled]="pagination.currentPage <= 1"
              class="text-xs text-gray-600 hover:text-gray-900 disabled:text-gray-300 disabled:cursor-not-allowed"
            >
              Previous
            </button>
            <span class="text-xs text-gray-500">
              Page {{ pagination.currentPage }} of {{ pagination.totalPages }}
            </span>
            <button
              (click)="goToPage(pagination.currentPage + 1)"
              [disabled]="pagination.currentPage >= pagination.totalPages"
              class="text-xs text-gray-600 hover:text-gray-900 disabled:text-gray-300 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      height: 100%;
      width: 360px;
      min-width: 320px;
    }
  `],
})
export class ConversationListComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);

  conversations$ = this.store.select(selectAllConversations);
  loading$ = this.store.select(selectConversationsLoading);
  filters$ = this.store.select(selectConversationFilters);
  pagination$ = this.store.select(selectConversationsPagination);
  selectedId$ = this.store.select(selectSelectedConversationId);

  searchQuery = '';
  activeOwnerTab = 'mine';
  activeStatus = '';

  ownerTabs = [
    { label: 'Mine', value: 'mine' },
    { label: 'Unassigned', value: 'unassigned' },
    { label: 'All', value: 'all' },
  ];

  statuses = [
    ConversationStatus.Open,
    ConversationStatus.Pending,
    ConversationStatus.Resolved,
    ConversationStatus.Snoozed,
  ];

  ngOnInit(): void {
    this.loadConversations();
  }

  loadConversations(): void {
    this.store.dispatch(
      ConversationsActions.loadConversations({
        filters: {
          assigneeType: this.activeOwnerTab,
          status: this.activeStatus || undefined,
          query: this.searchQuery || undefined,
        },
      }),
    );
  }

  setOwnerFilter(value: string): void {
    this.activeOwnerTab = value;
    this.loadConversations();
  }

  onStatusChange(status: string): void {
    this.activeStatus = status;
    this.loadConversations();
  }

  onSearch(): void {
    this.loadConversations();
  }

  selectConversation(id: number): void {
    this.router.navigate(['/conversations', id]);
  }

  goToPage(page: number): void {
    this.store.dispatch(
      ConversationsActions.loadConversations({
        filters: {
          assigneeType: this.activeOwnerTab,
          status: this.activeStatus || undefined,
          query: this.searchQuery || undefined,
          page,
        },
      }),
    );
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  getLastMessagePreview(conversation: Conversation): string {
    if (conversation.messages?.length) {
      const lastMsg = conversation.messages[conversation.messages.length - 1];
      return lastMsg.content?.slice(0, 80) || 'No content';
    }
    return 'No messages yet';
  }

  formatTime(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'now';
    if (diffMins < 60) return `${diffMins}m`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h`;
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d`;
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  getStatusClasses(status: ConversationStatus): string {
    switch (status) {
      case ConversationStatus.Open:
        return 'bg-green-100 text-green-800';
      case ConversationStatus.Pending:
        return 'bg-yellow-100 text-yellow-800';
      case ConversationStatus.Resolved:
        return 'bg-gray-100 text-gray-800';
      case ConversationStatus.Snoozed:
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  getPriorityColor(priority: ConversationPriority): string {
    switch (priority) {
      case ConversationPriority.Urgent:
        return 'text-red-600 font-semibold';
      case ConversationPriority.High:
        return 'text-orange-600 font-medium';
      case ConversationPriority.Medium:
        return 'text-yellow-600';
      case ConversationPriority.Low:
        return 'text-gray-500';
      default:
        return 'text-gray-400';
    }
  }
}
