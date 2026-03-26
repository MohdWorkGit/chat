import { Component, inject, EventEmitter, Output, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ConversationsActions } from '@app/store/conversations/conversations.actions';

@Component({
  selector: 'app-bulk-actions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (selectedIds.length > 0) {
      <div class="flex items-center gap-3 px-4 py-3 bg-blue-50 border-b border-blue-100">
        <div class="flex items-center gap-2">
          <input
            type="checkbox"
            [checked]="allSelected"
            (change)="toggleSelectAll()"
            class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          <span class="text-sm font-medium text-blue-700">
            {{ selectedIds.length }} selected
          </span>
        </div>

        <div class="h-5 w-px bg-blue-200"></div>

        <!-- Status Actions -->
        <div class="flex items-center gap-1">
          <button
            (click)="bulkUpdateStatus('resolved')"
            class="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-green-700 bg-green-100 hover:bg-green-200 rounded-md transition-colors"
          >
            <svg class="h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
            </svg>
            Resolve
          </button>
          <button
            (click)="bulkUpdateStatus('open')"
            class="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-yellow-700 bg-yellow-100 hover:bg-yellow-200 rounded-md transition-colors"
          >
            Reopen
          </button>
          <button
            (click)="bulkUpdateStatus('pending')"
            class="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md transition-colors"
          >
            Pending
          </button>
        </div>

        <div class="h-5 w-px bg-blue-200"></div>

        <!-- Assign -->
        <div class="relative">
          <button
            (click)="showAssignDropdown = !showAssignDropdown"
            class="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-md transition-colors"
          >
            <svg class="h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" />
            </svg>
            Assign
          </button>
          @if (showAssignDropdown) {
            <div class="absolute top-full left-0 mt-1 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-10">
              <input
                [(ngModel)]="assignSearch"
                placeholder="Search agents..."
                class="w-full px-3 py-2 text-sm border-b border-gray-200 focus:outline-none"
              />
              @for (agent of filteredAgents; track agent.id) {
                <button
                  (click)="bulkAssign(agent.id)"
                  class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-100"
                >
                  {{ agent.name }}
                </button>
              }
            </div>
          }
        </div>

        <!-- Label -->
        <div class="relative">
          <button
            (click)="showLabelDropdown = !showLabelDropdown"
            class="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-md transition-colors"
          >
            <svg class="h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9.568 3H5.25A2.25 2.25 0 0 0 3 5.25v4.318c0 .597.237 1.17.659 1.591l9.581 9.581c.699.699 1.78.872 2.607.33a18.095 18.095 0 0 0 5.223-5.223c.542-.827.369-1.908-.33-2.607L11.16 3.66A2.25 2.25 0 0 0 9.568 3Z" />
            </svg>
            Label
          </button>
          @if (showLabelDropdown) {
            <div class="absolute top-full left-0 mt-1 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-10">
              @for (label of availableLabels; track label.id) {
                <button
                  (click)="bulkAddLabel(label.title)"
                  class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 flex items-center gap-2"
                >
                  <span class="h-2.5 w-2.5 rounded-full" [style.background-color]="label.color"></span>
                  {{ label.title }}
                </button>
              }
            </div>
          }
        </div>

        <div class="flex-1"></div>

        <!-- Clear Selection -->
        <button
          (click)="clearSelection()"
          class="text-xs text-gray-500 hover:text-gray-700 transition-colors"
        >
          Clear selection
        </button>
      </div>
    }
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class BulkActionsComponent {
  private store = inject(Store);

  @Input() selectedIds: number[] = [];
  @Input() allSelected = false;
  @Input() availableLabels: Array<{ id: number; title: string; color: string }> = [];
  @Input() availableAgents: Array<{ id: number; name: string }> = [];

  @Output() selectionCleared = new EventEmitter<void>();
  @Output() selectAllToggled = new EventEmitter<void>();

  showAssignDropdown = false;
  showLabelDropdown = false;
  assignSearch = '';

  get filteredAgents() {
    if (!this.assignSearch) return this.availableAgents;
    const q = this.assignSearch.toLowerCase();
    return this.availableAgents.filter((a) => a.name.toLowerCase().includes(q));
  }

  toggleSelectAll(): void {
    this.selectAllToggled.emit();
  }

  clearSelection(): void {
    this.selectionCleared.emit();
    this.showAssignDropdown = false;
    this.showLabelDropdown = false;
  }

  bulkUpdateStatus(status: string): void {
    for (const id of this.selectedIds) {
      this.store.dispatch(ConversationsActions.updateConversationStatus({ id, status }));
    }
    this.clearSelection();
  }

  bulkAssign(agentId: number): void {
    for (const id of this.selectedIds) {
      this.store.dispatch(ConversationsActions.assignConversation({ id, assigneeId: agentId }));
    }
    this.showAssignDropdown = false;
    this.clearSelection();
  }

  bulkAddLabel(label: string): void {
    // Would dispatch a bulk label action
    this.showLabelDropdown = false;
    this.clearSelection();
  }
}
