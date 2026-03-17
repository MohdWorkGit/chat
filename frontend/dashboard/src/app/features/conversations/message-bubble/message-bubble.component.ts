import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Message, MessageType } from '@core/models/conversation.model';

@Component({
  selector: 'app-message-bubble',
  standalone: true,
  imports: [CommonModule],
  template: `
    @switch (message.messageType) {
      @case ('activity') {
        <!-- Activity message -->
        <div class="flex justify-center my-3">
          <span class="text-xs text-gray-400 bg-gray-50 rounded-full px-3 py-1">
            {{ message.content }}
          </span>
        </div>
      }
      @default {
        <!-- Regular message -->
        <div
          class="flex mb-3"
          [class.justify-end]="isOutgoing"
          [class.justify-start]="!isOutgoing"
        >
          <div class="flex items-end gap-2 max-w-[75%]" [class.flex-row-reverse]="isOutgoing">
            <!-- Avatar -->
            <div class="flex-shrink-0 mb-1">
              @if (message.senderAvatar) {
                <img
                  [src]="message.senderAvatar"
                  [alt]="message.senderName"
                  class="h-7 w-7 rounded-full object-cover"
                />
              } @else {
                <div class="h-7 w-7 rounded-full flex items-center justify-center text-xs font-medium text-white"
                  [class]="isOutgoing ? 'bg-blue-400' : 'bg-gray-400'">
                  {{ getInitials(message.senderName || '?') }}
                </div>
              }
            </div>

            <!-- Bubble -->
            <div>
              <!-- Sender name -->
              <p class="text-xs text-gray-500 mb-0.5" [class.text-right]="isOutgoing">
                {{ message.senderName || 'Unknown' }}
                <span class="text-gray-400 ml-1">{{ formatTime(message.createdAt) }}</span>
              </p>

              <div
                class="rounded-2xl px-4 py-2 text-sm break-words"
                [class]="getBubbleClasses()"
              >
                @if (message.private) {
                  <span class="text-xs font-medium text-yellow-700 block mb-1">Private Note</span>
                }

                <div [innerHTML]="message.content"></div>

                <!-- Attachments -->
                @if (message.attachments?.length) {
                  <div class="mt-2 space-y-1">
                    @for (attachment of message.attachments; track attachment.id) {
                      <a
                        [href]="attachment.fileUrl"
                        target="_blank"
                        rel="noopener"
                        class="flex items-center gap-2 px-2 py-1 rounded bg-white/50 hover:bg-white/80 text-xs transition-colors"
                      >
                        <svg class="h-4 w-4 flex-shrink-0" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" d="m18.375 12.739-7.693 7.693a4.5 4.5 0 0 1-6.364-6.364l10.94-10.94A3 3 0 1 1 19.5 7.372L8.552 18.32m.009-.01-.01.01m5.699-9.941-7.81 7.81a1.5 1.5 0 0 0 2.112 2.13" />
                        </svg>
                        <span class="truncate">{{ attachment.fileName }}</span>
                        <span class="text-gray-400 flex-shrink-0">{{ formatFileSize(attachment.fileSize) }}</span>
                      </a>
                    }
                  </div>
                }
              </div>
            </div>
          </div>
        </div>
      }
    }
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class MessageBubbleComponent {
  @Input({ required: true }) message!: Message;

  get isOutgoing(): boolean {
    return this.message.messageType === MessageType.Outgoing;
  }

  getBubbleClasses(): string {
    if (this.message.private) {
      return 'bg-yellow-50 border border-yellow-200 text-yellow-900';
    }
    if (this.isOutgoing) {
      return 'bg-blue-600 text-white rounded-br-sm';
    }
    return 'bg-gray-100 text-gray-900 rounded-bl-sm';
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
    return date.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' });
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}
