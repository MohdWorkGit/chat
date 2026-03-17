import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { ContactsActions } from '@app/store/contacts/contacts.actions';
import {
  selectSelectedContact,
  selectContactsLoading,
  selectContactsError,
} from '@app/store/contacts/contacts.selectors';
import { Contact } from '@core/models/contact.model';

@Component({
  selector: 'app-contact-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <svg class="animate-spin h-6 w-6 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
          </svg>
        </div>
      }

      @if (contact$ | async; as contact) {
        <!-- Header -->
        <div class="bg-white border-b border-gray-200">
          <div class="max-w-4xl mx-auto px-6 py-6">
            <div class="flex items-center justify-between mb-4">
              <a routerLink="/contacts" class="text-sm text-blue-600 hover:text-blue-500 flex items-center gap-1">
                <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
                </svg>
                Back to Contacts
              </a>
              <div class="flex items-center gap-2">
                <button
                  (click)="showMergeModal = true"
                  class="px-3 py-1.5 text-sm text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Merge Contact
                </button>
                <button
                  (click)="deleteContact(contact.id)"
                  class="px-3 py-1.5 text-sm text-red-600 border border-red-300 rounded-lg hover:bg-red-50 transition-colors"
                >
                  Delete
                </button>
              </div>
            </div>

            <div class="flex items-center gap-4">
              @if (contact.avatar) {
                <img [src]="contact.avatar" class="h-16 w-16 rounded-full object-cover" />
              } @else {
                <div class="h-16 w-16 rounded-full bg-gray-300 flex items-center justify-center text-xl font-medium text-white">
                  {{ getInitials(contact.name) }}
                </div>
              }
              <div>
                @if (!isEditing) {
                  <h1 class="text-2xl font-bold text-gray-900">{{ contact.name }}</h1>
                  <p class="text-sm text-gray-500">
                    {{ contact.email || 'No email' }}
                    @if (contact.phone) {
                      <span class="ml-2">{{ contact.phone }}</span>
                    }
                  </p>
                  <span
                    class="inline-flex items-center mt-1 px-2 py-0.5 rounded text-xs font-medium"
                    [class]="contact.contactType === 'customer' ? 'bg-green-100 text-green-800' : 'bg-blue-100 text-blue-800'"
                  >
                    {{ contact.contactType | titlecase }}
                  </span>
                } @else {
                  <p class="text-sm text-gray-500">Editing contact...</p>
                }
              </div>
              @if (!isEditing) {
                <button
                  (click)="startEditing(contact)"
                  class="ml-auto px-3 py-1.5 text-sm text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 transition-colors"
                >
                  Edit
                </button>
              }
            </div>
          </div>
        </div>

        <div class="max-w-4xl mx-auto px-6 py-6 space-y-6">
          <!-- Edit Form -->
          @if (isEditing) {
            <div class="bg-white rounded-lg border border-gray-200 p-6">
              <h3 class="text-sm font-semibold text-gray-900 mb-4">Edit Contact</h3>
              <form [formGroup]="editForm" (ngSubmit)="saveContact(contact.id)" class="space-y-4">
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Name</label>
                    <input
                      formControlName="name"
                      class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Email</label>
                    <input
                      formControlName="email"
                      type="email"
                      class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Phone</label>
                    <input
                      formControlName="phone"
                      class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Type</label>
                    <select
                      formControlName="contactType"
                      class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    >
                      <option value="lead">Lead</option>
                      <option value="customer">Customer</option>
                    </select>
                  </div>
                </div>
                <div class="flex items-center gap-2 pt-2">
                  <button
                    type="submit"
                    [disabled]="editForm.invalid"
                    class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 transition-colors"
                  >
                    Save Changes
                  </button>
                  <button
                    type="button"
                    (click)="cancelEditing()"
                    class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          }

          <!-- Custom Attributes -->
          @if (hasCustomAttributes(contact)) {
            <div class="bg-white rounded-lg border border-gray-200 p-6">
              <h3 class="text-sm font-semibold text-gray-900 mb-3">Custom Attributes</h3>
              <div class="grid grid-cols-2 gap-3">
                @for (attr of getCustomAttributes(contact); track attr.key) {
                  <div>
                    <label class="text-xs font-medium text-gray-500">{{ attr.key }}</label>
                    <p class="text-sm text-gray-900">{{ attr.value }}</p>
                  </div>
                }
              </div>
            </div>
          }

          <!-- Previous Conversations -->
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-3">
              Previous Conversations
              <span class="text-gray-400 font-normal ml-1">({{ contact.conversationsCount }})</span>
            </h3>
            @if (contact.conversationsCount > 0) {
              <p class="text-sm text-gray-500">
                This contact has {{ contact.conversationsCount }} conversation(s).
                View them in the conversations panel.
              </p>
            } @else {
              <p class="text-sm text-gray-400">No conversations yet.</p>
            }
          </div>

          <!-- Notes -->
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-3">Notes</h3>
            <div class="space-y-3">
              @for (note of notes; track $index) {
                <div class="bg-gray-50 rounded-lg p-3">
                  <p class="text-sm text-gray-700">{{ note.content }}</p>
                  <p class="text-xs text-gray-400 mt-1">{{ note.createdAt }}</p>
                </div>
              } @empty {
                <p class="text-sm text-gray-400">No notes yet.</p>
              }

              <div class="flex gap-2">
                <input
                  [(ngModel)]="newNote"
                  placeholder="Add a note..."
                  class="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  (keydown.enter)="addNote()"
                />
                <button
                  (click)="addNote()"
                  [disabled]="!newNote.trim()"
                  class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 transition-colors"
                >
                  Add
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Merge Modal (placeholder) -->
        @if (showMergeModal) {
          <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div class="bg-white rounded-xl shadow-xl max-w-md w-full mx-4 p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-2">Merge Contact</h3>
              <p class="text-sm text-gray-500 mb-4">
                Search for another contact to merge with {{ contact.name }}.
              </p>
              <input
                placeholder="Search by name or email..."
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 mb-4"
              />
              <div class="flex justify-end gap-2">
                <button
                  (click)="showMergeModal = false"
                  class="px-4 py-2 text-sm text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
                >
                  Cancel
                </button>
                <button
                  class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
                >
                  Merge
                </button>
              </div>
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
export class ContactDetailComponent implements OnInit {
  private store = inject(Store);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  contact$ = this.store.select(selectSelectedContact);
  loading$ = this.store.select(selectContactsLoading);
  error$ = this.store.select(selectContactsError);

  isEditing = false;
  showMergeModal = false;
  newNote = '';
  notes: { content: string; createdAt: string }[] = [];

  editForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    email: ['', Validators.email],
    phone: [''],
    contactType: ['lead'],
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.store.dispatch(ContactsActions.loadContact({ id }));
      this.store.dispatch(ContactsActions.selectContact({ id }));
    }
  }

  startEditing(contact: Contact): void {
    this.isEditing = true;
    this.editForm.patchValue({
      name: contact.name,
      email: contact.email || '',
      phone: contact.phone || '',
      contactType: contact.contactType,
    });
  }

  cancelEditing(): void {
    this.isEditing = false;
  }

  saveContact(id: number): void {
    if (this.editForm.invalid) return;
    this.store.dispatch(ContactsActions.updateContact({ id, data: this.editForm.value }));
    this.isEditing = false;
  }

  deleteContact(id: number): void {
    if (confirm('Are you sure you want to delete this contact? This action cannot be undone.')) {
      this.store.dispatch(ContactsActions.deleteContact({ id }));
      this.router.navigate(['/contacts']);
    }
  }

  addNote(): void {
    const content = this.newNote.trim();
    if (!content) return;
    this.notes.unshift({
      content,
      createdAt: new Date().toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
      }),
    });
    this.newNote = '';
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  hasCustomAttributes(contact: Contact): boolean {
    return Object.keys(contact.customAttributes || {}).length > 0;
  }

  getCustomAttributes(contact: Contact): { key: string; value: string }[] {
    return Object.entries(contact.customAttributes || {}).map(([key, value]) => ({
      key,
      value: String(value),
    }));
  }
}
