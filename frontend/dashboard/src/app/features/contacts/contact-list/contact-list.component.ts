import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { ContactsActions } from '@app/store/contacts/contacts.actions';
import {
  selectAllContacts,
  selectContactsLoading,
  selectContactsPagination,
} from '@app/store/contacts/contacts.selectors';
import { Contact } from '@core/models/contact.model';

@Component({
  selector: 'app-contact-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col bg-white">
      <!-- Header -->
      <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200">
        <h1 class="text-xl font-semibold text-gray-900">Contacts</h1>
        <button
          (click)="openNewContact()"
          class="inline-flex items-center gap-1.5 px-3 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          New Contact
        </button>
      </div>

      <!-- Search -->
      <div class="px-6 py-3 border-b border-gray-200">
        <div class="relative max-w-md">
          <svg class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
          </svg>
          <input
            type="text"
            [(ngModel)]="searchQuery"
            (input)="onSearch()"
            placeholder="Search contacts by name, email, or phone..."
            class="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
      </div>

      <!-- Table -->
      <div class="flex-1 overflow-auto">
        @if (loading$ | async) {
          <div class="flex items-center justify-center py-12">
            <svg class="animate-spin h-6 w-6 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
            </svg>
          </div>
        } @else {
          <table class="w-full">
            <thead>
              <tr class="bg-gray-50 border-b border-gray-200">
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Phone</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Last Activity</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Conversations</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              @for (contact of contacts$ | async; track contact.id) {
                <tr
                  (click)="navigateToContact(contact.id)"
                  class="hover:bg-gray-50 cursor-pointer transition-colors"
                >
                  <td class="px-6 py-3">
                    <div class="flex items-center gap-3">
                      @if (contact.avatar) {
                        <img [src]="contact.avatar" class="h-8 w-8 rounded-full object-cover" />
                      } @else {
                        <div class="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center text-xs font-medium text-white">
                          {{ getInitials(contact.name) }}
                        </div>
                      }
                      <span class="text-sm font-medium text-gray-900">{{ contact.name }}</span>
                    </div>
                  </td>
                  <td class="px-6 py-3 text-sm text-gray-500">{{ contact.email || '-' }}</td>
                  <td class="px-6 py-3 text-sm text-gray-500">{{ contact.phone || '-' }}</td>
                  <td class="px-6 py-3">
                    <span
                      class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                      [class]="contact.contactType === 'customer' ? 'bg-green-100 text-green-800' : 'bg-blue-100 text-blue-800'"
                    >
                      {{ contact.contactType | titlecase }}
                    </span>
                  </td>
                  <td class="px-6 py-3 text-sm text-gray-500">
                    {{ contact.lastActivityAt ? formatTime(contact.lastActivityAt) : 'Never' }}
                  </td>
                  <td class="px-6 py-3 text-sm text-gray-500">{{ contact.conversationsCount }}</td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="6" class="px-6 py-12 text-center text-sm text-gray-400">
                    @if (searchQuery) {
                      No contacts found matching "{{ searchQuery }}"
                    } @else {
                      No contacts yet. Click "New Contact" to add one.
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        }
      </div>

      <!-- Pagination -->
      @if (pagination$ | async; as pagination) {
        @if (pagination.totalPages > 1) {
          <div class="flex items-center justify-between px-6 py-3 border-t border-gray-200 bg-gray-50">
            <span class="text-sm text-gray-500">
              {{ pagination.totalCount }} total contacts
            </span>
            <div class="flex items-center gap-2">
              <button
                (click)="goToPage(pagination.currentPage - 1)"
                [disabled]="pagination.currentPage <= 1"
                class="px-3 py-1 text-sm border border-gray-300 rounded-md hover:bg-gray-100 disabled:text-gray-300 disabled:cursor-not-allowed transition-colors"
              >
                Previous
              </button>
              <span class="text-sm text-gray-500">
                Page {{ pagination.currentPage }} of {{ pagination.totalPages }}
              </span>
              <button
                (click)="goToPage(pagination.currentPage + 1)"
                [disabled]="pagination.currentPage >= pagination.totalPages"
                class="px-3 py-1 text-sm border border-gray-300 rounded-md hover:bg-gray-100 disabled:text-gray-300 disabled:cursor-not-allowed transition-colors"
              >
                Next
              </button>
            </div>
          </div>
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
export class ContactListComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);

  contacts$ = this.store.select(selectAllContacts);
  loading$ = this.store.select(selectContactsLoading);
  pagination$ = this.store.select(selectContactsPagination);

  searchQuery = '';
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.store.dispatch(ContactsActions.loadContacts({}));
  }

  onSearch(): void {
    if (this.searchTimeout) clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      if (this.searchQuery.trim()) {
        this.store.dispatch(ContactsActions.searchContacts({ query: this.searchQuery.trim() }));
      } else {
        this.store.dispatch(ContactsActions.loadContacts({}));
      }
    }, 300);
  }

  navigateToContact(id: number): void {
    this.router.navigate(['/contacts', id]);
  }

  openNewContact(): void {
    this.router.navigate(['/contacts', 'new']);
  }

  goToPage(page: number): void {
    this.store.dispatch(ContactsActions.loadContacts({ filters: { page } }));
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  formatTime(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 7) return `${diffDays} days ago`;
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
