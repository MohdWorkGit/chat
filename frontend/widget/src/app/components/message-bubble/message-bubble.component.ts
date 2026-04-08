import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  selector: 'cew-message-bubble',
  standalone: true,
  imports: [CommonModule, DatePipe],
  template: `
    <div class="message-bubble" [ngClass]="senderType">
      <div class="message-content">{{ content }}</div>
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
}
