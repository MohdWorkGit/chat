import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { InboxesActions } from '@store/inboxes/inboxes.actions';
import { selectAllInboxes, selectInboxesLoading } from '@store/inboxes/inboxes.selectors';

@Component({
  selector: 'app-inbox-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Inboxes</h2>
        <a
          routerLink="new"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          Create Inbox
        </a>
      </div>

      <!-- Loading State -->
      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((inboxes$ | async); as inboxes) {
          @if (inboxes.length === 0) {
            <div class="text-center py-12">
              <svg class="mx-auto h-12 w-12 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 13.5h3.86a2.25 2.25 0 0 1 2.012 1.244l.256.512a2.25 2.25 0 0 0 2.013 1.244h3.218a2.25 2.25 0 0 0 2.013-1.244l.256-.512a2.25 2.25 0 0 1 2.013-1.244h3.859m-19.5.338V18a2.25 2.25 0 0 0 2.25 2.25h15A2.25 2.25 0 0 0 21.75 18v-4.162c0-.224-.034-.447-.1-.661L19.24 5.338a2.25 2.25 0 0 0-2.15-1.588H6.911a2.25 2.25 0 0 0-2.15 1.588L2.35 13.177a2.25 2.25 0 0 0-.1.661Z" />
              </svg>
              <p class="mt-2 text-sm text-gray-500">No inboxes configured yet.</p>
              <a
                routerLink="new"
                class="mt-4 inline-block px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-700"
              >
                Create your first inbox
              </a>
            </div>
          } @else {
            <!-- Inbox Table -->
            <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Channel</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Auto Assignment</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Members</th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                  @for (inbox of inboxes; track inbox.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
                      <td class="px-6 py-4">
                        <a [routerLink]="[inbox.id]" class="text-sm font-medium text-gray-900 hover:text-blue-600">
                          {{ inbox.name }}
                        </a>
                      </td>
                      <td class="px-6 py-4">
                        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                          {{ inbox.channelType }}
                        </span>
                      </td>
                      <td class="px-6 py-4">
                        <span
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          [class]="inbox.enableAutoAssignment ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'"
                        >
                          {{ inbox.enableAutoAssignment ? 'Enabled' : 'Disabled' }}
                        </span>
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500">
                        {{ inbox.members?.length || 0 }} members
                      </td>
                      <td class="px-6 py-4 text-right">
                        <a
                          [routerLink]="[inbox.id]"
                          class="text-sm text-blue-600 hover:text-blue-800 font-medium"
                        >
                          Settings
                        </a>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
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
export class InboxListComponent implements OnInit {
  private store = inject(Store);

  inboxes$ = this.store.select(selectAllInboxes);
  loading$ = this.store.select(selectInboxesLoading);

  ngOnInit(): void {
    this.store.dispatch(InboxesActions.loadInboxes());
  }
}
