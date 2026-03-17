import {
  Component,
  ChangeDetectionStrategy,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MessageBubbleComponent } from '../message-bubble/message-bubble.component';
import { PreChatFormComponent, PreChatFormData } from '../pre-chat-form/pre-chat-form.component';
import { CsatSurveyComponent, CsatData } from '../csat-survey/csat-survey.component';
import { TypingIndicatorComponent } from '../typing-indicator/typing-indicator.component';
import { WidgetApiService, Message, Conversation } from '../../services/widget-api.service';
import { SignalrService } from '../../services/signalr.service';

type ChatView = 'pre-chat' | 'conversation' | 'csat';

@Component({
  selector: 'cew-chat-window',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MessageBubbleComponent,
    PreChatFormComponent,
    CsatSurveyComponent,
    TypingIndicatorComponent,
  ],
  template: `
    <div class="chat-window">
      <div class="chat-header">
        <div class="chat-header-info">
          <div class="agent-avatar">
            {{ agentInitials() }}
          </div>
          <div>
            <div style="font-weight: 600; font-size: 15px;">{{ agentName() }}</div>
            <div style="font-size: 12px; opacity: 0.85;">{{ agentStatus() }}</div>
          </div>
        </div>
        <button
          style="background: none; border: none; color: white; cursor: pointer; padding: 4px;"
          (click)="close.emit()"
          aria-label="Minimize chat">
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19 13H5v-2h14v2z"/>
          </svg>
        </button>
      </div>

      @switch (currentView()) {
        @case ('pre-chat') {
          <cew-pre-chat-form (formSubmit)="onPreChatSubmit($event)" />
        }
        @case ('conversation') {
          <div class="chat-messages" #messageList>
            @for (message of messages(); track message.id) {
              <cew-message-bubble
                [content]="message.content"
                [senderType]="message.senderType"
                [timestamp]="message.createdAt" />
            }
            @if (isAgentTyping()) {
              <cew-typing-indicator />
            }
          </div>

          <div class="chat-input-area">
            <textarea
              [(ngModel)]="newMessage"
              (keydown.enter)="onSendMessage($event)"
              placeholder="Type a message..."
              rows="1"
              [attr.aria-label]="'Message input'">
            </textarea>
            <button
              class="send-button"
              (click)="sendMessage()"
              [disabled]="!newMessage.trim()"
              aria-label="Send message">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
              </svg>
            </button>
          </div>
        }
        @case ('csat') {
          <cew-csat-survey
            [conversationId]="conversationId()"
            (surveySubmit)="onCsatSubmit($event)" />
        }
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatWindowComponent implements OnInit, OnDestroy {
  @Input() websiteToken = '';
  @Input() locale = 'en';
  @Output() close = new EventEmitter<void>();

  currentView = signal<ChatView>('pre-chat');
  messages = signal<Message[]>([]);
  conversationId = signal<number>(0);
  agentName = signal('Support');
  agentStatus = signal('We typically reply within a few minutes');
  isAgentTyping = signal(false);
  newMessage = '';

  agentInitials = computed(() => {
    const name = this.agentName();
    return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
  });

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly apiService: WidgetApiService,
    private readonly signalrService: SignalrService,
  ) {}

  ngOnInit(): void {
    this.signalrService.messages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        this.messages.update(msgs => [...msgs, message]);
      });

    this.signalrService.typing$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isTyping => {
        this.isAgentTyping.set(isTyping);
      });

    this.signalrService.conversationResolved$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentView.set('csat');
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onPreChatSubmit(data: PreChatFormData): void {
    this.apiService.createConversation(this.websiteToken, {
      name: data.name,
      email: data.email,
      customFields: data.customFields,
    }).subscribe({
      next: (conversation: Conversation) => {
        this.conversationId.set(conversation.id);
        this.signalrService.joinConversation(conversation.id);
        this.currentView.set('conversation');
      },
    });
  }

  onSendMessage(event: Event): void {
    event.preventDefault();
    this.sendMessage();
  }

  sendMessage(): void {
    const content = this.newMessage.trim();
    if (!content || !this.conversationId()) return;

    this.apiService.sendMessage(this.websiteToken, this.conversationId(), content)
      .subscribe({
        next: (message: Message) => {
          this.messages.update(msgs => [...msgs, message]);
          this.newMessage = '';
        },
      });
  }

  onCsatSubmit(data: CsatData): void {
    this.apiService.submitCsat(this.websiteToken, this.conversationId(), data)
      .subscribe({
        next: () => {
          this.currentView.set('pre-chat');
          this.messages.set([]);
          this.conversationId.set(0);
        },
      });
  }
}
