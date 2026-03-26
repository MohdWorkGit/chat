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
import { FileUploadComponent } from '../file-upload/file-upload.component';
import { EmojiPickerComponent } from '../emoji-picker/emoji-picker.component';
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
    FileUploadComponent,
    EmojiPickerComponent,
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

          @if (showFileUpload()) {
            <cew-file-upload (fileSelected)="onFileSelected($event)" />
          }

          <div class="chat-input-area" style="position: relative;">
            @if (showEmojiPicker()) {
              <cew-emoji-picker (emojiSelected)="onEmojiSelected($event)" />
            }
            <button
              class="input-action-btn"
              (click)="toggleEmojiPicker()"
              [class.active]="showEmojiPicker()"
              aria-label="Toggle emoji picker">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                <path d="M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z"/>
              </svg>
            </button>
            <button
              class="input-action-btn"
              (click)="toggleFileUpload()"
              [class.active]="showFileUpload()"
              aria-label="Attach file">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                <path d="M16.5 6v11.5c0 2.21-1.79 4-4 4s-4-1.79-4-4V5c0-1.38 1.12-2.5 2.5-2.5s2.5 1.12 2.5 2.5v10.5c0 .55-.45 1-1 1s-1-.45-1-1V6H10v9.5c0 1.38 1.12 2.5 2.5 2.5s2.5-1.12 2.5-2.5V5c0-2.21-1.79-4-4-4S7 2.79 7 5v12.5c0 3.04 2.46 5.5 5.5 5.5s5.5-2.46 5.5-5.5V6h-1.5z"/>
              </svg>
            </button>
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
  showEmojiPicker = signal(false);
  showFileUpload = signal(false);
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

  toggleEmojiPicker(): void {
    this.showEmojiPicker.update(v => !v);
    if (this.showEmojiPicker()) {
      this.showFileUpload.set(false);
    }
  }

  toggleFileUpload(): void {
    this.showFileUpload.update(v => !v);
    if (this.showFileUpload()) {
      this.showEmojiPicker.set(false);
    }
  }

  onEmojiSelected(emoji: string): void {
    this.newMessage += emoji;
    this.showEmojiPicker.set(false);
  }

  onFileSelected(file: File): void {
    if (!this.conversationId()) return;

    this.apiService.uploadAttachment(this.conversationId(), file)
      .subscribe({
        next: (message: Message) => {
          this.messages.update(msgs => [...msgs, message]);
          this.showFileUpload.set(false);
        },
      });
  }
}
