import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { WidgetAttachment } from '../../services/widget-api.service';

@Component({
  selector: 'cew-message-bubble',
  standalone: true,
  imports: [CommonModule, DatePipe],
  template: `
    <div class="message-bubble" [ngClass]="senderType">
      @if (content) {
        <div class="message-content">{{ content }}</div>
      }
      @if (attachments?.length) {
        <div class="attachments">
          @for (att of attachments; track att.id) {
            @if (isImage(att)) {
              <a
                class="attachment-image"
                [href]="att.fileUrl || '#'"
                target="_blank"
                rel="noopener noreferrer">
                <img [src]="att.fileUrl" [alt]="att.fileName || 'image'" loading="lazy" />
              </a>
            } @else {
              <a
                class="attachment-file"
                [href]="att.fileUrl || '#'"
                target="_blank"
                rel="noopener noreferrer"
                [download]="att.fileName || ''">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M14 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm4 18H6V4h7v5h5v11z"/>
                </svg>
                <span class="attachment-name">{{ att.fileName || 'file' }}</span>
                <span class="attachment-size">{{ formatSize(att.fileSize) }}</span>
              </a>
            }
          }
        </div>
      }
      <div class="message-time" [ngClass]="senderType">
        {{ timestamp | date:'shortTime' }}
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: flex;
      font-family: var(--widget-font, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif);
    }
    .message-bubble {
      max-width: 78%;
      padding: 10px 14px;
      border-radius: 18px;
      font-size: 14px;
      line-height: 1.5;
      word-wrap: break-word;
      animation: msg-fade-in 0.24s cubic-bezier(0.16, 1, 0.3, 1);
      font-family: inherit;
    }
    @keyframes msg-fade-in {
      from { opacity: 0; transform: translateY(6px) scale(0.98); }
      to { opacity: 1; transform: translateY(0) scale(1); }
    }
    .message-bubble.agent {
      background-color: var(--widget-bubble-agent, #ffffff);
      color: var(--widget-text, #1f2937);
      align-self: flex-start;
      border-bottom-left-radius: 6px;
      border: 1px solid rgba(17, 24, 39, 0.06);
      box-shadow: 0 1px 2px rgba(17, 24, 39, 0.04);
    }
    .message-bubble.customer {
      background: linear-gradient(135deg, var(--widget-bubble-customer, #1b72e8) 0%, var(--widget-primary-hover, #1560c7) 100%);
      color: var(--widget-bubble-customer-text, #ffffff);
      align-self: flex-end;
      margin-left: auto;
      border-bottom-right-radius: 6px;
      box-shadow: 0 2px 6px rgba(27, 114, 232, 0.22);
    }
    .message-content {
      white-space: pre-wrap;
    }
    .attachments {
      display: flex;
      flex-direction: column;
      gap: 6px;
      margin-top: 6px;
    }
    .attachment-image img {
      max-width: 220px;
      max-height: 220px;
      border-radius: 10px;
      display: block;
    }
    .attachment-file {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 8px 10px;
      border-radius: 10px;
      background: rgba(255, 255, 255, 0.14);
      color: inherit;
      text-decoration: none;
      font-size: 13px;
      max-width: 220px;
    }
    .message-bubble.agent .attachment-file {
      background: rgba(17, 24, 39, 0.05);
    }
    .attachment-name {
      flex: 1;
      min-width: 0;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      font-weight: 500;
    }
    .attachment-size {
      opacity: 0.7;
      font-size: 11px;
      flex-shrink: 0;
    }
    .message-time {
      font-size: 10px;
      margin-top: 4px;
      opacity: 0.75;
      font-weight: 500;
      letter-spacing: 0.02em;
    }
    .message-time.agent {
      color: var(--widget-text-secondary, #6b7280);
    }
    .message-time.customer {
      color: rgba(255, 255, 255, 0.82);
      text-align: right;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageBubbleComponent {
  @Input() content = '';
  @Input() senderType: 'agent' | 'customer' = 'agent';
  @Input() timestamp: string | Date = new Date();
  @Input() attachments: WidgetAttachment[] | null | undefined = null;

  isImage(att: WidgetAttachment): boolean {
    if (att.fileType === 'image') return true;
    return !!att.contentType?.startsWith('image/');
  }

  formatSize(bytes: number): string {
    if (!bytes) return '';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}
