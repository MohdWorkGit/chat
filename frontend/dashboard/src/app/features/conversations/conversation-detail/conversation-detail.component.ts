import { Component, inject, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, combineLatest, map, of, switchMap, takeUntil } from 'rxjs';
import { ConversationsActions } from '@app/store/conversations/conversations.actions';
import {
  selectSelectedConversation,
  selectConversationMessages,
  selectMessagesLoading,
  selectSelectedConversationId,
} from '@app/store/conversations/conversations.selectors';
import { Conversation, Message, ConversationStatus } from '@core/models/conversation.model';
import { MessageBubbleComponent } from '../message-bubble/message-bubble.component';
import { ReplyBoxComponent } from '../reply-box/reply-box.component';
import { CopilotPanelComponent } from '@app/features/captain/copilot-panel/copilot-panel.component';

@Component({
  selector: 'app-conversation-detail',
  standalone: true,
  imports: [CommonModule, MessageBubbleComponent, ReplyBoxComponent, CopilotPanelComponent],
  template: `
    @if (conversation$ | async; as conversation) {
      <div class="flex h-full">
        <!-- Main conversation panel -->
        <div class="flex flex-col flex-1 min-w-0">
          <!-- Header -->
          <div class="flex items-center justify-between px-4 py-3 border-b border-gray-200 bg-white">
            <div class="flex items-center gap-3">
              @if (conversation.contact?.avatar) {
                <img [src]="conversation.contact!.avatar" class="h-9 w-9 rounded-full object-cover" />
              } @else {
                <div class="h-9 w-9 rounded-full bg-gray-300 flex items-center justify-center text-sm font-medium text-white">
                  {{ getInitials(conversation.contact?.name || 'U') }}
                </div>
              }
              <div>
                <h3 class="text-sm font-semibold text-gray-900">
                  {{ conversation.contact?.name || 'Unknown Contact' }}
                </h3>
                <p class="text-xs text-gray-500">
                  #{{ conversation.displayId }}
                  @if (conversation.contact?.email) {
                    <span class="ml-1">{{ conversation.contact!.email }}</span>
                  }
                </p>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex items-center gap-1">
              <!-- Status -->
              <select
                [value]="conversation.status"
                (change)="onStatusChange(conversation.id, $event)"
                class="text-xs border border-gray-300 rounded-md px-2 py-1 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                @for (status of statuses; track status) {
                  <option [value]="status">{{ status | titlecase }}</option>
                }
              </select>

              <!-- Resolve -->
              <button
                (click)="updateStatus(conversation.id, 'resolved')"
                class="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium rounded-md transition-colors"
                [class]="conversation.status === 'resolved'
                  ? 'bg-gray-100 text-gray-500'
                  : 'bg-green-100 text-green-700 hover:bg-green-200'"
              >
                <svg class="h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
                </svg>
                Resolve
              </button>

              <!-- Snooze -->
              <button
                (click)="updateStatus(conversation.id, 'snoozed')"
                class="p-1.5 text-gray-400 hover:text-gray-600 rounded transition-colors"
                title="Snooze"
              >
                <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                </svg>
              </button>

              <!-- Mute -->
              <button
                class="p-1.5 text-gray-400 hover:text-gray-600 rounded transition-colors"
                title="Mute notifications"
              >
                <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M17.25 9.75 19.5 12m0 0 2.25 2.25M19.5 12l2.25-2.25M19.5 12l-2.25 2.25m-10.5-6 4.72-4.72a.75.75 0 0 1 1.28.53v15.88a.75.75 0 0 1-1.28.53l-4.72-4.72H4.51c-.88 0-1.704-.507-1.938-1.354A9.009 9.009 0 0 1 2.25 12c0-.83.112-1.633.322-2.396C2.806 8.756 3.63 8.25 4.51 8.25H6.75Z" />
                </svg>
              </button>

              <!-- Priority toggle -->
              <button
                (click)="togglePriority(conversation)"
                class="p-1.5 rounded transition-colors"
                [class]="conversation.priority !== 'none'
                  ? 'text-orange-500 hover:text-orange-600'
                  : 'text-gray-400 hover:text-gray-600'"
                title="Toggle priority"
              >
                <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M3 3v1.5M3 21v-6m0 0 2.77-.693a9 9 0 0 1 6.208.682l.108.054a9 9 0 0 0 6.086.71l3.114-.732a48.524 48.524 0 0 1-.005-10.499l-3.11.732a9 9 0 0 1-6.085-.711l-.108-.054a9 9 0 0 0-6.208-.682L3 4.5M3 15V4.5" />
                </svg>
              </button>

              <!-- Copilot toggle -->
              <button
                (click)="showCopilotPanel = !showCopilotPanel"
                class="p-1.5 rounded transition-colors"
                [class]="showCopilotPanel ? 'text-purple-600 bg-purple-50' : 'text-gray-400 hover:text-gray-600'"
                title="Copilot"
              >
                <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09zM18.259 8.715L18 9.75l-.259-1.035a3.375 3.375 0 00-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 002.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 002.455 2.456L21.75 6l-1.036.259a3.375 3.375 0 00-2.455 2.456z" />
                </svg>
              </button>

              <!-- Contact sidebar toggle -->
              <button
                (click)="showContactPanel = !showContactPanel"
                class="p-1.5 text-gray-400 hover:text-gray-600 rounded transition-colors"
                title="Contact info"
              >
                <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Assignee bar -->
          @if (conversation.assignee) {
            <div class="flex items-center gap-2 px-4 py-1.5 bg-gray-50 border-b border-gray-200 text-xs text-gray-500">
              <span>Assigned to</span>
              <span class="font-medium text-gray-700">{{ conversation.assignee.name }}</span>
              @if (conversation.team) {
                <span class="text-gray-400">in {{ conversation.team.name }}</span>
              }
            </div>
          }

          <!-- Messages -->
          <div #messagesContainer class="flex-1 overflow-y-auto px-4 py-4">
            @if (messagesLoading$ | async) {
              <div class="flex justify-center py-4">
                <svg class="animate-spin h-5 w-5 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                </svg>
              </div>
            }

            @for (message of messages$ | async; track message.id) {
              <app-message-bubble [message]="message" />
            } @empty {
              @if (!(messagesLoading$ | async)) {
                <div class="flex flex-col items-center justify-center h-full text-gray-400">
                  <p class="text-sm">No messages yet</p>
                </div>
              }
            }

            <!-- Typing indicator -->
            @if (isTyping) {
              <div class="flex items-center gap-2 ml-9 mb-2">
                <div class="flex gap-1 bg-gray-100 rounded-full px-3 py-2">
                  <div class="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style="animation-delay: 0ms"></div>
                  <div class="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style="animation-delay: 150ms"></div>
                  <div class="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style="animation-delay: 300ms"></div>
                </div>
              </div>
            }
          </div>

          <!-- Reply Box -->
          <app-reply-box (messageSent)="onMessageSent(conversation.id, $event)" />
        </div>

        <!-- Copilot panel -->
        @if (showCopilotPanel) {
          <app-copilot-panel
            [conversationId]="conversation.id"
            (closed)="showCopilotPanel = false"
          />
        }

        <!-- Contact sidebar panel -->
        @if (showContactPanel && conversation.contact) {
          <div class="w-72 border-l border-gray-200 bg-white overflow-y-auto flex-shrink-0">
            <div class="p-4">
              <div class="flex items-center justify-between mb-4">
                <h4 class="text-sm font-semibold text-gray-900">Contact Info</h4>
                <button (click)="showContactPanel = false" class="text-gray-400 hover:text-gray-600">
                  <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>

              <!-- Contact avatar and name -->
              <div class="text-center mb-4">
                @if (conversation.contact.avatar) {
                  <img [src]="conversation.contact.avatar" class="h-16 w-16 rounded-full object-cover mx-auto mb-2" />
                } @else {
                  <div class="h-16 w-16 rounded-full bg-gray-300 flex items-center justify-center text-lg font-medium text-white mx-auto mb-2">
                    {{ getInitials(conversation.contact.name || 'U') }}
                  </div>
                }
                <h3 class="text-sm font-semibold text-gray-900">{{ conversation.contact.name }}</h3>
              </div>

              <!-- Details -->
              <div class="space-y-3 text-sm">
                @if (conversation.contact.email) {
                  <div>
                    <label class="text-xs font-medium text-gray-500">Email</label>
                    <p class="text-gray-900">{{ conversation.contact.email }}</p>
                  </div>
                }
                @if (conversation.contact.phone) {
                  <div>
                    <label class="text-xs font-medium text-gray-500">Phone</label>
                    <p class="text-gray-900">{{ conversation.contact.phone }}</p>
                  </div>
                }

                <div>
                  <label class="text-xs font-medium text-gray-500">Conversation</label>
                  <p class="text-gray-900">#{{ conversation.displayId }}</p>
                </div>

                @if (conversation.inbox) {
                  <div>
                    <label class="text-xs font-medium text-gray-500">Inbox</label>
                    <p class="text-gray-900">{{ conversation.inbox.name }} ({{ conversation.inbox.channelType }})</p>
                  </div>
                }

                @if (conversation.labels.length) {
                  <div>
                    <label class="text-xs font-medium text-gray-500 block mb-1">Labels</label>
                    <div class="flex flex-wrap gap-1">
                      @for (label of conversation.labels; track label) {
                        <span class="inline-flex items-center px-2 py-0.5 rounded text-xs bg-gray-100 text-gray-700">
                          {{ label }}
                        </span>
                      }
                    </div>
                  </div>
                }

                <div>
                  <label class="text-xs font-medium text-gray-500">Created</label>
                  <p class="text-gray-900">{{ formatDate(conversation.createdAt) }}</p>
                </div>

                <!-- Custom attributes -->
                @if (hasCustomAttributes(conversation)) {
                  <div class="border-t border-gray-200 pt-3 mt-3">
                    <label class="text-xs font-medium text-gray-500 block mb-2">Custom Attributes</label>
                    @for (entry of getCustomAttributes(conversation); track entry.key) {
                      <div class="mb-2">
                        <label class="text-xs text-gray-400">{{ entry.key }}</label>
                        <p class="text-xs text-gray-900">{{ entry.value }}</p>
                      </div>
                    }
                  </div>
                }
              </div>
            </div>
          </div>
        }
      </div>
    } @else {
      <!-- No conversation selected -->
      <div class="flex flex-col items-center justify-center h-full bg-gray-50 text-gray-400">
        <svg class="h-16 w-16 mb-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M8.625 12a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm0 0H8.25m4.125 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm0 0H12m4.125 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm0 0h-.375M21 12c0 4.556-4.03 8.25-9 8.25a9.764 9.764 0 0 1-2.555-.337A5.972 5.972 0 0 1 5.41 20.97a5.969 5.969 0 0 1-.474-.065 4.48 4.48 0 0 0 .978-2.025c.09-.457-.133-.901-.467-1.226C3.93 16.178 3 14.189 3 12c0-4.556 4.03-8.25 9-8.25s9 3.694 9 8.25Z" />
        </svg>
        <p class="text-lg font-medium">Select a conversation</p>
        <p class="text-sm mt-1">Choose a conversation from the list to start chatting</p>
      </div>
    }
  `,
  styles: [`
    :host {
      display: flex;
      flex: 1;
      height: 100%;
      min-width: 0;
    }
  `],
})
export class ConversationDetailComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  private store = inject(Store);
  private route = inject(ActivatedRoute);
  private destroy$ = new Subject<void>();

  conversation$: Observable<Conversation | null> = this.store.select(selectSelectedConversation);
  messagesLoading$ = this.store.select(selectMessagesLoading);

  messages$!: Observable<Message[]>;
  showContactPanel = false;
  showCopilotPanel = false;
  isTyping = false;
  private shouldScrollToBottom = true;

  statuses = [
    ConversationStatus.Open,
    ConversationStatus.Pending,
    ConversationStatus.Resolved,
    ConversationStatus.Snoozed,
  ];

  ngOnInit(): void {
    // Sync route :id param into the store so selecting a conversation in the
    // list (which navigates here) actually loads its messages.
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const idStr = params.get('id');
      const id = idStr ? Number(idStr) : null;
      this.store.dispatch(ConversationsActions.selectConversation({ id }));
      if (id !== null && !Number.isNaN(id)) {
        this.store.dispatch(ConversationsActions.loadMessages({ conversationId: id }));
      }
    });

    // Read messages from the dedicated messages slice populated by
    // loadMessagesSuccess; fall back to any messages embedded on the
    // conversation entity for conversations loaded before that slice is hydrated.
    this.messages$ = this.store.select(selectSelectedConversationId).pipe(
      switchMap((id) => {
        if (!id) return of<Message[]>([]);
        return combineLatest([
          this.store.select(selectConversationMessages(id)),
          this.store.select(selectSelectedConversation),
        ]).pipe(
          map(([stateMessages, conversation]) =>
            stateMessages.length ? stateMessages : conversation?.messages ?? [],
          ),
        );
      }),
    );

    // Subscribe to re-enable auto-scroll on new messages
    this.messages$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.shouldScrollToBottom = true;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  private scrollToBottom(): void {
    try {
      const el = this.messagesContainer?.nativeElement;
      if (el) {
        el.scrollTop = el.scrollHeight;
      }
    } catch (_) {
      // Element might not exist yet
    }
  }

  onStatusChange(id: number, event: Event): void {
    const status = (event.target as HTMLSelectElement).value;
    this.store.dispatch(ConversationsActions.updateConversationStatus({ id, status }));
  }

  updateStatus(id: number, status: string): void {
    this.store.dispatch(ConversationsActions.updateConversationStatus({ id, status }));
  }

  togglePriority(conversation: Conversation): void {
    // Cycle through priorities: none -> high -> urgent -> none
    const cycle: Record<string, string> = { none: 'high', high: 'urgent', urgent: 'none' };
    const next = cycle[conversation.priority] || 'none';
    this.store.dispatch(
      ConversationsActions.conversationUpdated({
        conversationId: conversation.id,
        updates: { priority: next },
      }),
    );
  }

  onMessageSent(conversationId: number, event: { content: string; isPrivate: boolean }): void {
    this.store.dispatch(
      ConversationsActions.sendMessage({
        conversationId,
        content: event.content,
        isPrivate: event.isPrivate,
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

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
    });
  }

  hasCustomAttributes(conversation: Conversation): boolean {
    return Object.keys(conversation.customAttributes || {}).length > 0;
  }

  getCustomAttributes(conversation: Conversation): { key: string; value: string }[] {
    return Object.entries(conversation.customAttributes || {}).map(([key, value]) => ({
      key,
      value: String(value),
    }));
  }
}
