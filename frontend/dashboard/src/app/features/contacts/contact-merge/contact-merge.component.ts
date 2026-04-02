import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { ContactService } from '@core/services/contact.service';
import { ContactsActions } from '@app/store/contacts/contacts.actions';
import { selectSelectedContact } from '@app/store/contacts/contacts.selectors';
import { Contact } from '@core/models/contact.model';

@Component({
  selector: 'app-contact-merge',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-4xl mx-auto px-6 py-6">
        <!-- Header -->
        <div class="mb-6">
          <a routerLink="/contacts" class="text-sm text-blue-600 hover:text-blue-500 flex items-center gap-1 mb-2">
            <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
            </svg>
            Back to Contacts
          </a>
          <h1 class="text-xl font-semibold text-gray-900">Merge Contacts</h1>
          <p class="text-sm text-gray-500 mt-1">
            Select a primary contact and a secondary contact to merge. The primary contact's fields take priority.
          </p>
        </div>

        <!-- Two Panel Layout -->
        <div class="grid grid-cols-2 gap-6 mb-6">
          <!-- Primary Contact Panel -->
          <div class="bg-white rounded-lg border-2 transition-colors"
               [class]="primaryContact ? 'border-blue-500' : 'border-gray-200'">
            <div class="px-4 py-3 border-b border-gray-200 bg-blue-50 rounded-t-lg">
              <h3 class="text-sm font-semibold text-blue-800">Primary Contact</h3>
              <p class="text-xs text-blue-600">Fields from this contact take priority</p>
            </div>
            <div class="p-4">
              @if (!primaryContact) {
                <div class="mb-3">
                  <input
                    [(ngModel)]="primarySearch"
                    (input)="searchPrimary()"
                    placeholder="Search by name or email..."
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
                @if (primaryResults.length > 0) {
                  <div class="space-y-1 max-h-48 overflow-y-auto">
                    @for (contact of primaryResults; track contact.id) {
                      <button
                        (click)="selectPrimary(contact)"
                        class="w-full text-left px-3 py-2 rounded-lg hover:bg-gray-50 flex items-center gap-3 transition-colors"
                      >
                        <div class="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center text-xs font-medium text-blue-700 flex-shrink-0">
                          {{ getInitials(contact.name) }}
                        </div>
                        <div class="min-w-0">
                          <p class="text-sm font-medium text-gray-900 truncate">{{ contact.name }}</p>
                          <p class="text-xs text-gray-500 truncate">{{ contact.email || 'No email' }}</p>
                        </div>
                      </button>
                    }
                  </div>
                } @else if (primarySearch.length > 0) {
                  <p class="text-sm text-gray-400 text-center py-4">No contacts found</p>
                } @else {
                  <p class="text-sm text-gray-400 text-center py-4">Search to select a contact</p>
                }
              } @else {
                <div class="flex items-center gap-3 mb-4">
                  <div class="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center text-sm font-medium text-blue-700">
                    {{ getInitials(primaryContact.name) }}
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-semibold text-gray-900">{{ primaryContact.name }}</p>
                    <p class="text-xs text-gray-500">{{ primaryContact.email || 'No email' }}</p>
                  </div>
                  <button
                    (click)="clearPrimary()"
                    class="text-xs text-red-500 hover:text-red-700 transition-colors"
                  >
                    Remove
                  </button>
                </div>
                <div class="space-y-2 text-sm">
                  <div class="flex justify-between">
                    <span class="text-gray-500">Phone</span>
                    <span class="text-gray-900">{{ primaryContact.phone || '-' }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500">Type</span>
                    <span class="text-gray-900 capitalize">{{ primaryContact.contactType }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500">Company</span>
                    <span class="text-gray-900">{{ primaryContact.company?.name || '-' }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500">Conversations</span>
                    <span class="text-gray-900">{{ primaryContact.conversationsCount }}</span>
                  </div>
                </div>
              }
            </div>
          </div>

          <!-- Secondary Contact Panel -->
          <div class="bg-white rounded-lg border-2 transition-colors"
               [class]="secondaryContact ? 'border-orange-500' : 'border-gray-200'">
            <div class="px-4 py-3 border-b border-gray-200 bg-orange-50 rounded-t-lg">
              <h3 class="text-sm font-semibold text-orange-800">Secondary Contact</h3>
              <p class="text-xs text-orange-600">This contact will be merged and deleted</p>
            </div>
            <div class="p-4">
              @if (!secondaryContact) {
                <div class="mb-3">
                  <input
                    [(ngModel)]="secondarySearch"
                    (input)="searchSecondary()"
                    placeholder="Search by name or email..."
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
                @if (secondaryResults.length > 0) {
                  <div class="space-y-1 max-h-48 overflow-y-auto">
                    @for (contact of secondaryResults; track contact.id) {
                      <button
                        (click)="selectSecondary(contact)"
                        [disabled]="primaryContact?.id === contact.id"
                        class="w-full text-left px-3 py-2 rounded-lg hover:bg-gray-50 flex items-center gap-3 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                      >
                        <div class="h-8 w-8 rounded-full bg-orange-100 flex items-center justify-center text-xs font-medium text-orange-700 flex-shrink-0">
                          {{ getInitials(contact.name) }}
                        </div>
                        <div class="min-w-0">
                          <p class="text-sm font-medium text-gray-900 truncate">{{ contact.name }}</p>
                          <p class="text-xs text-gray-500 truncate">{{ contact.email || 'No email' }}</p>
                        </div>
                      </button>
                    }
                  </div>
                } @else if (secondarySearch.length > 0) {
                  <p class="text-sm text-gray-400 text-center py-4">No contacts found</p>
                } @else {
                  <p class="text-sm text-gray-400 text-center py-4">Search to select a contact</p>
                }
              } @else {
                <div class="flex items-center gap-3 mb-4">
                  <div class="h-10 w-10 rounded-full bg-orange-100 flex items-center justify-center text-sm font-medium text-orange-700">
                    {{ getInitials(secondaryContact.name) }}
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-semibold text-gray-900">{{ secondaryContact.name }}</p>
                    <p class="text-xs text-gray-500">{{ secondaryContact.email || 'No email' }}</p>
                  </div>
                  <button
                    (click)="clearSecondary()"
                    class="text-xs text-red-500 hover:text-red-700 transition-colors"
                  >
                    Remove
                  </button>
                </div>
                <div class="space-y-2 text-sm">
                  <div class="flex justify-between">
                    <span class="text-gray-500">Phone</span>
                    <span class="text-gray-900">{{ secondaryContact.phone || '-' }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500">Type</span>
                    <span class="text-gray-900 capitalize">{{ secondaryContact.contactType }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500">Company</span>
                    <span class="text-gray-900">{{ secondaryContact.company?.name || '-' }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-500">Conversations</span>
                    <span class="text-gray-900">{{ secondaryContact.conversationsCount }}</span>
                  </div>
                </div>
              }
            </div>
          </div>
        </div>

        <!-- Merge Preview -->
        @if (primaryContact && secondaryContact) {
          <div class="bg-white rounded-lg border border-gray-200 mb-6">
            <div class="px-4 py-3 border-b border-gray-200">
              <h3 class="text-sm font-semibold text-gray-900">Merge Preview</h3>
              <p class="text-xs text-gray-500">The resulting contact after merge</p>
            </div>
            <div class="p-4">
              <div class="grid grid-cols-3 gap-4 text-sm">
                <div class="font-medium text-gray-500">Field</div>
                <div class="font-medium text-gray-500">Result Value</div>
                <div class="font-medium text-gray-500">Source</div>

                @for (field of mergePreviewFields; track field.name) {
                  <div class="text-gray-700">{{ field.name }}</div>
                  <div class="text-gray-900 font-medium">{{ field.value || '-' }}</div>
                  <div>
                    <span
                      class="inline-flex px-2 py-0.5 rounded text-xs font-medium"
                      [class]="field.source === 'primary' ? 'bg-blue-100 text-blue-700' : 'bg-orange-100 text-orange-700'"
                    >
                      {{ field.source }}
                    </span>
                  </div>
                }
              </div>

              <div class="mt-4 p-3 bg-yellow-50 rounded-lg border border-yellow-200">
                <p class="text-xs text-yellow-800">
                  <strong>Note:</strong> All conversations ({{ primaryContact.conversationsCount + secondaryContact.conversationsCount }} total)
                  and notes from both contacts will be combined. The secondary contact will be permanently deleted.
                </p>
              </div>
            </div>
          </div>

          <!-- Merge Actions -->
          <div class="flex items-center gap-3">
            <button
              (click)="executeMerge()"
              [disabled]="merging"
              class="px-6 py-2.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
            >
              @if (merging) {
                <svg class="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                </svg>
                Merging...
              } @else {
                Merge Contacts
              }
            </button>
            <button
              (click)="resetSelection()"
              class="px-4 py-2.5 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
            >
              Reset
            </button>
          </div>
        }

        <!-- Success Message -->
        @if (mergeComplete) {
          <div class="mt-6 p-4 bg-green-50 border border-green-200 rounded-lg">
            <div class="flex items-center gap-2">
              <svg class="h-5 w-5 text-green-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
              </svg>
              <p class="text-sm font-medium text-green-800">Contacts merged successfully!</p>
            </div>
            <a routerLink="/contacts" class="mt-2 inline-block text-sm text-green-700 hover:text-green-600 underline">
              Return to contacts list
            </a>
          </div>
        }

        <!-- Error Message -->
        @if (mergeError) {
          <div class="mt-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p class="text-sm text-red-700">{{ mergeError }}</p>
          </div>
        }
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
export class ContactMergeComponent implements OnInit {
  private readonly contactService = inject(ContactService);
  private readonly store = inject(Store);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  primarySearch = '';
  secondarySearch = '';
  primaryResults: Contact[] = [];
  secondaryResults: Contact[] = [];
  primaryContact: Contact | null = null;
  secondaryContact: Contact | null = null;
  merging = false;
  mergeComplete = false;
  mergeError = '';

  ngOnInit(): void {
    const contactId = Number(this.route.snapshot.queryParamMap.get('contactId'));
    if (contactId) {
      this.store.select(selectSelectedContact).subscribe((contact) => {
        if (contact && contact.id === contactId) {
          this.primaryContact = contact;
        }
      });
    }
  }

  searchPrimary(): void {
    if (this.primarySearch.length < 2) {
      this.primaryResults = [];
      return;
    }
    this.contactService.search(this.primarySearch).subscribe((result) => {
      this.primaryResults = result.data;
    });
  }

  searchSecondary(): void {
    if (this.secondarySearch.length < 2) {
      this.secondaryResults = [];
      return;
    }
    this.contactService.search(this.secondarySearch).subscribe((result) => {
      this.secondaryResults = result.data;
    });
  }

  selectPrimary(contact: Contact): void {
    this.primaryContact = contact;
    this.primarySearch = '';
    this.primaryResults = [];
  }

  selectSecondary(contact: Contact): void {
    this.secondaryContact = contact;
    this.secondarySearch = '';
    this.secondaryResults = [];
  }

  clearPrimary(): void {
    this.primaryContact = null;
    this.mergeComplete = false;
    this.mergeError = '';
  }

  clearSecondary(): void {
    this.secondaryContact = null;
    this.mergeComplete = false;
    this.mergeError = '';
  }

  resetSelection(): void {
    this.primaryContact = null;
    this.secondaryContact = null;
    this.primarySearch = '';
    this.secondarySearch = '';
    this.mergeComplete = false;
    this.mergeError = '';
  }

  get mergePreviewFields(): Array<{ name: string; value: string; source: string }> {
    if (!this.primaryContact || !this.secondaryContact) return [];
    return [
      {
        name: 'Name',
        value: this.primaryContact.name,
        source: 'primary',
      },
      {
        name: 'Email',
        value: this.primaryContact.email || this.secondaryContact.email || '',
        source: this.primaryContact.email ? 'primary' : 'secondary',
      },
      {
        name: 'Phone',
        value: this.primaryContact.phone || this.secondaryContact.phone || '',
        source: this.primaryContact.phone ? 'primary' : 'secondary',
      },
      {
        name: 'Type',
        value: this.primaryContact.contactType,
        source: 'primary',
      },
      {
        name: 'Company',
        value: this.primaryContact.company?.name || this.secondaryContact.company?.name || '',
        source: this.primaryContact.company ? 'primary' : 'secondary',
      },
      {
        name: 'Location',
        value: this.primaryContact.location || this.secondaryContact.location || '',
        source: this.primaryContact.location ? 'primary' : 'secondary',
      },
    ];
  }

  executeMerge(): void {
    if (!this.primaryContact || !this.secondaryContact) return;
    this.merging = true;
    this.mergeError = '';

    this.contactService.merge(this.primaryContact.id, this.secondaryContact.id).subscribe({
      next: (mergedContact) => {
        this.merging = false;
        this.mergeComplete = true;
        this.store.dispatch(ContactsActions.loadContact({ id: mergedContact.id }));
      },
      error: (err) => {
        this.merging = false;
        this.mergeError = err?.error?.message || 'An error occurred while merging contacts. Please try again.';
      },
    });
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}
