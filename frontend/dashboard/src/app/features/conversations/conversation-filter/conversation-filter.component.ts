import { Component, inject, EventEmitter, Output, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ConversationsActions } from '@app/store/conversations/conversations.actions';
import { selectAllInboxes } from '@store/inboxes/inboxes.selectors';
import { selectAllTeams } from '@store/teams/teams.selectors';
import { selectAllLabels } from '@store/labels/labels.selectors';

@Component({
  selector: 'app-conversation-filter',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    @if (isOpen) {
      <div class="border-b border-gray-200 bg-gray-50 p-4">
        <div class="flex items-center justify-between mb-3">
          <h4 class="text-sm font-medium text-gray-700">Advanced Filters</h4>
          <button (click)="close()" class="text-gray-400 hover:text-gray-600">
            <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <form [formGroup]="filterForm" (ngSubmit)="applyFilters()" class="space-y-3">
          <div class="grid grid-cols-2 gap-3 lg:grid-cols-4">
            <!-- Status -->
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Status</label>
              <select formControlName="status" class="w-full rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
                <option value="">All</option>
                <option value="open">Open</option>
                <option value="resolved">Resolved</option>
                <option value="pending">Pending</option>
                <option value="snoozed">Snoozed</option>
              </select>
            </div>

            <!-- Priority -->
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Priority</label>
              <select formControlName="priority" class="w-full rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
                <option value="">All</option>
                <option value="none">None</option>
                <option value="low">Low</option>
                <option value="medium">Medium</option>
                <option value="high">High</option>
                <option value="urgent">Urgent</option>
              </select>
            </div>

            <!-- Inbox -->
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Inbox</label>
              <select formControlName="inboxId" class="w-full rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
                <option value="">All Inboxes</option>
                @for (inbox of inboxes$ | async; track inbox.id) {
                  <option [value]="inbox.id">{{ inbox.name }}</option>
                }
              </select>
            </div>

            <!-- Team -->
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Team</label>
              <select formControlName="teamId" class="w-full rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
                <option value="">All Teams</option>
                @for (team of teams$ | async; track team.id) {
                  <option [value]="team.id">{{ team.name }}</option>
                }
              </select>
            </div>

            <!-- Label -->
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Label</label>
              <select formControlName="label" class="w-full rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
                <option value="">All Labels</option>
                @for (label of labels$ | async; track label.id) {
                  <option [value]="label.title">{{ label.title }}</option>
                }
              </select>
            </div>

            <!-- Date From -->
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Created After</label>
              <input
                type="date"
                formControlName="dateFrom"
                class="w-full rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <!-- Date To -->
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Created Before</label>
              <input
                type="date"
                formControlName="dateTo"
                class="w-full rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>

            <!-- Assignee Type -->
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Assignment</label>
              <select formControlName="assigneeType" class="w-full rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
                <option value="">All</option>
                <option value="me">Assigned to me</option>
                <option value="unassigned">Unassigned</option>
              </select>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex items-center gap-2 pt-2">
            <button
              type="submit"
              class="px-4 py-1.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
            >
              Apply Filters
            </button>
            <button
              type="button"
              (click)="resetFilters()"
              class="px-4 py-1.5 text-sm font-medium text-gray-600 hover:text-gray-800 transition-colors"
            >
              Reset
            </button>

            <!-- Save as Custom View -->
            @if (!showSaveView) {
              <button
                type="button"
                (click)="showSaveView = true"
                class="px-4 py-1.5 text-sm font-medium text-blue-600 hover:text-blue-800 transition-colors"
              >
                Save as View
              </button>
            } @else {
              <div class="flex items-center gap-2 ml-2">
                <input
                  [(value)]="viewName"
                  (input)="viewName = $any($event.target).value"
                  placeholder="View name..."
                  class="rounded-lg border border-gray-300 px-2.5 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
                <button
                  type="button"
                  (click)="saveView()"
                  class="px-3 py-1.5 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-lg transition-colors"
                >
                  Save
                </button>
                <button
                  type="button"
                  (click)="showSaveView = false"
                  class="text-gray-400 hover:text-gray-600"
                >
                  Cancel
                </button>
              </div>
            }
          </div>
        </form>
      </div>
    }
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class ConversationFilterComponent {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  @Input() isOpen = false;
  @Output() closed = new EventEmitter<void>();
  @Output() filtersApplied = new EventEmitter<Record<string, any>>();

  inboxes$ = this.store.select(selectAllInboxes);
  teams$ = this.store.select(selectAllTeams);
  labels$ = this.store.select(selectAllLabels);

  showSaveView = false;
  viewName = '';

  filterForm: FormGroup = this.fb.group({
    status: [''],
    priority: [''],
    inboxId: [''],
    teamId: [''],
    label: [''],
    dateFrom: [''],
    dateTo: [''],
    assigneeType: [''],
  });

  close(): void {
    this.closed.emit();
  }

  applyFilters(): void {
    const filters = this.filterForm.value;
    const cleaned: Record<string, any> = {};
    for (const [key, value] of Object.entries(filters)) {
      if (value) cleaned[key] = value;
    }
    this.filtersApplied.emit(cleaned);
    this.store.dispatch(ConversationsActions.setFilters({ filters: cleaned as any }));
  }

  resetFilters(): void {
    this.filterForm.reset();
    this.store.dispatch(ConversationsActions.clearFilters());
    this.filtersApplied.emit({});
  }

  saveView(): void {
    if (!this.viewName.trim()) return;
    // Would dispatch save custom filter action
    this.showSaveView = false;
    this.viewName = '';
  }
}
