import {
  Component,
  ChangeDetectionStrategy,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  AfterViewChecked,
  ViewChild,
  ElementRef,
  signal,
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
import { AgentAvailabilityComponent, AgentAvailabilityStatus } from '../agent-availability/agent-availability.component';
import { GreetingMessageComponent } from '../greeting-message/greeting-message.component';
import { WidgetApiService, Message, Conversation } from '../../services/widget-api.service';
import { SignalrService } from '../../services/signalr.service';

type ChatView = 'greeting' | 'pre-chat' | 'conversation' | 'csat';

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
    AgentAvailabilityComponent,
    GreetingMessageComponent,
  ],
  template: `
    <div class="chat-window">
      <div class="chat-header">
        <cew-agent-availability
          [name]="agentName()"
          [avatarUrl]="agentAvatarUrl()"
          [status]="agentAvailability()"
          [replyTimeMinutes]="agentReplyTime()" />
        <div class="header-actions">
          @if (currentView() === 'conversation') {
            <button
              class="header-action-btn close-conversation-btn"
              (click)="onEndConversation()"
              aria-label="End conversation">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
              </svg>
            </button>
          }
          <button
            class="header-action-btn minimize-btn"
            (click)="close.emit()"
            aria-label="Minimize chat">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19 13H5v-2h14v2z"/>
            </svg>
          </button>
        </div>
      </div>

      @switch (currentView()) {
        @case ('greeting') {
          <cew-greeting-message
            [teamName]="agentName()"
            [greetingTitle]="greetingTitle"
            [greetingText]="greetingText"
            (startConversation)="onStartConversation()" />
        }
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
  styles: [`
    :host {
      display: contents;
      font-family: var(--widget-font, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif);
    }
    :host([hidden]) {
      display: none !important;
    }
    .chat-window {
      width: 384px;
      height: 580px;
      max-height: calc(100vh - 120px);
      background: var(--widget-bg, #ffffff);
      border-radius: 16px;
      box-shadow: 0 12px 40px rgba(17, 24, 39, 0.18), 0 2px 10px rgba(17, 24, 39, 0.06);
      display: flex;
      flex-direction: column;
      overflow: hidden;
      animation: widget-slide-up 0.32s cubic-bezier(0.16, 1, 0.3, 1);
      font-family: inherit;
    }
    @keyframes widget-slide-up {
      from { opacity: 0; transform: translateY(16px) scale(0.98); }
      to { opacity: 1; transform: translateY(0) scale(1); }
    }
    .chat-header {
      position: relative;
      background: linear-gradient(135deg, var(--widget-primary, #1b72e8) 0%, #1560c7 55%, #0f4fa8 100%);
      color: #ffffff;
      padding: 18px 18px 20px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-shrink: 0;
      overflow: hidden;
    }
    .chat-header::before {
      content: '';
      position: absolute;
      inset: 0;
      background-image:
        radial-gradient(circle at 85% -20%, rgba(255, 255, 255, 0.18), transparent 55%),
        radial-gradient(circle at 20% 120%, rgba(255, 255, 255, 0.10), transparent 50%);
      pointer-events: none;
    }
    .chat-header > * {
      position: relative;
      z-index: 1;
    }
    .chat-messages {
      flex: 1;
      overflow-y: auto;
      padding: 18px 16px 12px;
      display: flex;
      flex-direction: column;
      gap: 10px;
      background-color: #f6f8fb;
      background-image:
        radial-gradient(circle at 0% 0%, rgba(27, 114, 232, 0.04), transparent 60%),
        radial-gradient(circle at 100% 100%, rgba(27, 114, 232, 0.03), transparent 55%);
      scroll-behavior: smooth;
    }
    .chat-messages::-webkit-scrollbar {
      width: 6px;
    }
    .chat-messages::-webkit-scrollbar-track {
      background: transparent;
    }
    .chat-messages::-webkit-scrollbar-thumb {
      background-color: rgba(17, 24, 39, 0.16);
      border-radius: 3px;
    }
    .chat-messages::-webkit-scrollbar-thumb:hover {
      background-color: rgba(17, 24, 39, 0.28);
    }
    .chat-input-area {
      padding: 10px 12px 12px;
      border-top: 1px solid var(--widget-border, #e5e7eb);
      display: flex;
      align-items: flex-end;
      gap: 6px;
      background: var(--widget-bg, #ffffff);
      flex-shrink: 0;
    }
    .chat-input-area textarea {
      flex: 1;
      border: 1px solid var(--widget-border, #e5e7eb);
      border-radius: 22px;
      padding: 10px 16px;
      font-family: inherit;
      font-size: 14px;
      color: var(--widget-text, #1f2937);
      background-color: #fafbfc;
      resize: none;
      outline: none;
      min-height: 40px;
      max-height: 120px;
      line-height: 1.4;
      transition: border-color 0.18s ease, background-color 0.18s ease, box-shadow 0.18s ease;
      -webkit-appearance: none;
      appearance: none;
    }
    .chat-input-area textarea::placeholder {
      color: #9ca3af;
    }
    .chat-input-area textarea:hover {
      border-color: #d1d5db;
    }
    .chat-input-area textarea:focus {
      border-color: var(--widget-primary, #1b72e8);
      background-color: #ffffff;
      box-shadow: 0 0 0 3px rgba(27, 114, 232, 0.12);
    }
    .send-button {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--widget-primary, #1b72e8) 0%, var(--widget-primary-hover, #1560c7) 100%);
      color: #ffffff;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      box-shadow: 0 2px 8px rgba(27, 114, 232, 0.3);
      transition: transform 0.15s ease, box-shadow 0.18s ease, opacity 0.18s ease;
    }
    .send-button svg {
      transform: translateX(1px);
    }
    .send-button:hover:not(:disabled) {
      transform: translateY(-1px) scale(1.03);
      box-shadow: 0 4px 12px rgba(27, 114, 232, 0.38);
    }
    .send-button:active:not(:disabled) {
      transform: translateY(0) scale(1);
    }
    .send-button:disabled {
      opacity: 0.4;
      cursor: not-allowed;
      box-shadow: none;
    }
    .input-action-btn {
      width: 38px;
      height: 38px;
      border-radius: 50%;
      background: none;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--widget-text-secondary, #6b7280);
      flex-shrink: 0;
      transition: background-color 0.15s ease, color 0.15s ease, transform 0.15s ease;
    }
    .input-action-btn:hover {
      background-color: rgba(17, 24, 39, 0.06);
      color: var(--widget-text, #1f2937);
      transform: scale(1.05);
    }
    .input-action-btn.active {
      color: var(--widget-primary, #1b72e8);
      background-color: rgba(27, 114, 232, 0.12);
    }
    .header-actions {
      display: flex;
      align-items: center;
      gap: 4px;
      flex-shrink: 0;
    }
    .header-action-btn {
      background: rgba(255, 255, 255, 0.12);
      border: none;
      color: #ffffff;
      cursor: pointer;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      transition: background-color 0.18s ease, transform 0.15s ease;
    }
    .header-action-btn:hover {
      background-color: rgba(255, 255, 255, 0.22);
      transform: scale(1.05);
    }
    .header-action-btn:active {
      transform: scale(0.96);
    }
    .close-conversation-btn:hover {
      background-color: rgba(239, 68, 68, 0.55);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatWindowComponent implements OnInit, OnDestroy, AfterViewChecked {
  @Input() websiteToken = '';
  @Input() locale = 'en';
  /**
   * Forwarded from the host page so the widget can reach a cross-origin
   * API. Configuration of the shared services is handled in AppComponent;
   * this component just accepts the input so the binding in the template
   * is valid.
   */
  @Input() apiBaseUrl = '';
  @Output() close = new EventEmitter<void>();

  currentView = signal<ChatView>('greeting');
  messages = signal<Message[]>([]);
  conversationId = signal<number>(0);
  agentName = signal('Support');
  agentAvatarUrl = signal('');
  agentAvailability = signal<AgentAvailabilityStatus>('online');
  agentReplyTime = signal(5);
  isAgentTyping = signal(false);
  showEmojiPicker = signal(false);
  showFileUpload = signal(false);
  newMessage = '';
  greetingTitle = 'Hi there!';
  greetingText = 'We are here to help. Ask us anything, or share your feedback.';

  @ViewChild('messageList') private messageList?: ElementRef<HTMLDivElement>;
  private shouldScrollToBottom = false;
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly apiService: WidgetApiService,
    private readonly signalrService: SignalrService,
  ) {}

  ngOnInit(): void {
    this.restoreConversation();

    this.signalrService.messages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        // Dedupe by id: the SignalR broadcast echoes back every message,
        // including ones the widget itself just POSTed (and optimistically
        // appended to the list). Without this guard those messages would
        // render twice.
        this.messages.update(msgs =>
          msgs.some(m => m.id === message.id) ? msgs : [...msgs, message],
        );
        this.shouldScrollToBottom = true;
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
        this.persistConversation();
      });
  }

  private get storageKey(): string {
    return `cew-widget-conversation:${this.websiteToken || 'default'}`;
  }

  private persistConversation(): void {
    try {
      const id = this.conversationId();
      if (!id) {
        localStorage.removeItem(this.storageKey);
        return;
      }
      localStorage.setItem(
        this.storageKey,
        JSON.stringify({ conversationId: id, view: this.currentView() }),
      );
    } catch {
      // localStorage may be unavailable (private mode, quota). Non-fatal.
    }
  }

  private clearPersistedConversation(): void {
    try {
      localStorage.removeItem(this.storageKey);
    } catch {
      // no-op
    }
  }

  private restoreConversation(): void {
    let saved: { conversationId: number; view: ChatView } | null = null;
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (raw) saved = JSON.parse(raw);
    } catch {
      saved = null;
    }
    if (!saved?.conversationId) return;

    this.conversationId.set(saved.conversationId);
    this.currentView.set(saved.view || 'conversation');
    this.signalrService.joinConversation(saved.conversationId);

    this.apiService.getMessages(this.websiteToken, saved.conversationId)
      .subscribe({
        next: (msgs: Message[]) => {
          this.messages.set(msgs);
          this.shouldScrollToBottom = true;
        },
        error: () => {
          // Saved conversation is no longer accessible — reset.
          this.clearPersistedConversation();
          this.conversationId.set(0);
          this.messages.set([]);
          this.currentView.set('greeting');
        },
      });
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onStartConversation(): void {
    this.currentView.set('pre-chat');
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
        this.persistConversation();
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
          this.messages.update(msgs =>
            msgs.some(m => m.id === message.id) ? msgs : [...msgs, message],
          );
          this.newMessage = '';
          this.shouldScrollToBottom = true;
        },
      });
  }

  onCsatSubmit(data: CsatData): void {
    this.apiService.submitCsat(this.websiteToken, this.conversationId(), data)
      .subscribe({
        next: () => {
          this.currentView.set('greeting');
          this.messages.set([]);
          this.conversationId.set(0);
          this.clearPersistedConversation();
        },
      });
  }

  onEndConversation(): void {
    const convId = this.conversationId();
    if (convId) {
      this.signalrService.leaveConversation(convId);
    }
    this.currentView.set('greeting');
    this.messages.set([]);
    this.conversationId.set(0);
    this.newMessage = '';
    this.showEmojiPicker.set(false);
    this.showFileUpload.set(false);
    this.isAgentTyping.set(false);
    this.clearPersistedConversation();
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

  private scrollToBottom(): void {
    const el = this.messageList?.nativeElement;
    if (el) {
      el.scrollTop = el.scrollHeight;
    }
  }

  onFileSelected(file: File): void {
    if (!this.conversationId()) return;

    this.apiService.uploadAttachment(this.websiteToken, this.conversationId(), file)
      .subscribe({
        next: (message: Message) => {
          this.messages.update(msgs =>
            msgs.some(m => m.id === message.id) ? msgs : [...msgs, message],
          );
          this.showFileUpload.set(false);
        },
      });
  }
}
