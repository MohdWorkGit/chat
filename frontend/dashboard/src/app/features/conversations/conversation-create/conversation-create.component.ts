import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { ConversationsActions } from '@app/store/conversations/conversations.actions';
import { selectAllContacts } from '@app/store/contacts/contacts.selectors';
import { selectAllInboxes } from '@app/store/inboxes/inboxes.selectors';
import { ContactsActions } from '@app/store/contacts/contacts.actions';
import { InboxesActions } from '@app/store/inboxes/inboxes.actions';

@Component({
  selector: 'app-conversation-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-2xl mx-auto px-6 py-6">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <div>
            <a routerLink="/conversations" class="text-sm text-blue-600 hover:text-blue-500 flex items-center gap-1 mb-2">
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
              </svg>
              Back to Conversations
            </a>
            <h1 class="text-xl font-semibold text-gray-900">New Conversation</h1>
          </div>
        </div>

        <!-- Form -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <form [formGroup]="conversationForm" (ngSubmit)="onSubmit()" class="space-y-5">
            <div class="grid grid-cols-2 gap-4">
              <!-- Contact -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Contact <span class="text-red-500">*</span></label>
                <select
                  formControlName="contactId"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="">Select a contact...</option>
                  @for (contact of contacts$ | async; track contact.id) {
                    <option [value]="contact.id">{{ contact.name }} {{ contact.email ? '(' + contact.email + ')' : '' }}</option>
                  }
                </select>
                @if (conversationForm.get('contactId')?.hasError('required') && conversationForm.get('contactId')?.touched) {
                  <p class="mt-1 text-xs text-red-500">Contact is required</p>
                }
              </div>

              <!-- Inbox -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Inbox <span class="text-red-500">*</span></label>
                <select
                  formControlName="inboxId"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="">Select an inbox...</option>
                  @for (inbox of inboxes$ | async; track inbox.id) {
                    <option [value]="inbox.id">{{ inbox.name }} ({{ inbox.channelType }})</option>
                  }
                </select>
                @if (conversationForm.get('inboxId')?.hasError('required') && conversationForm.get('inboxId')?.touched) {
                  <p class="mt-1 text-xs text-red-500">Inbox is required</p>
                }
              </div>
            </div>

            <!-- Subject -->
            <div>
              <label class="block text-sm font-medium text-gray-700">Subject</label>
              <input
                formControlName="subject"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Conversation subject"
              />
            </div>

            <!-- Initial Message -->
            <div>
              <label class="block text-sm font-medium text-gray-700">Initial Message <span class="text-red-500">*</span></label>
              <textarea
                formControlName="message"
                rows="4"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Type the initial message..."
              ></textarea>
              @if (conversationForm.get('message')?.hasError('required') && conversationForm.get('message')?.touched) {
                <p class="mt-1 text-xs text-red-500">Message is required</p>
              }
            </div>

            <div class="grid grid-cols-2 gap-4">
              <!-- Priority -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Priority</label>
                <select
                  formControlName="priority"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="none">None</option>
                  <option value="low">Low</option>
                  <option value="medium">Medium</option>
                  <option value="high">High</option>
                  <option value="urgent">Urgent</option>
                </select>
              </div>

              <!-- Assignee -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Assignee</label>
                <select
                  formControlName="assigneeId"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="">Unassigned</option>
                </select>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex items-center gap-3 pt-4 border-t border-gray-200">
              <button
                type="submit"
                [disabled]="conversationForm.invalid"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                Create Conversation
              </button>
              <button
                type="button"
                (click)="onCancel()"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
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
export class ConversationCreateComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  contacts$ = this.store.select(selectAllContacts);
  inboxes$ = this.store.select(selectAllInboxes);

  conversationForm: FormGroup = this.fb.group({
    contactId: ['', Validators.required],
    inboxId: ['', Validators.required],
    subject: [''],
    message: ['', Validators.required],
    priority: ['none'],
    assigneeId: [''],
  });

  ngOnInit(): void {
    this.store.dispatch(ContactsActions.loadContacts({}));
    this.store.dispatch(InboxesActions.loadInboxes());
  }

  onSubmit(): void {
    if (this.conversationForm.invalid) return;

    const { contactId, inboxId, subject, message, priority, assigneeId } = this.conversationForm.value;
    const data: Record<string, unknown> = {
      contactId: Number(contactId),
      inboxId: Number(inboxId),
      priority,
      status: 'open',
      additionalAttributes: {},
      customAttributes: {},
    };

    if (subject) {
      data['additionalAttributes'] = { subject };
    }

    if (assigneeId) {
      data['assigneeId'] = Number(assigneeId);
    }

    if (message) {
      data['message'] = { content: message };
    }

    this.store.dispatch(ConversationsActions.createConversation({ data }));
    this.router.navigate(['/conversations']);
  }

  onCancel(): void {
    this.router.navigate(['/conversations']);
  }
}
