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
    }
    .message-bubble {
      max-width: 75%;
      padding: 10px 14px;
      border-radius: 16px;
      font-size: 14px;
      line-height: 1.5;
      word-wrap: break-word;
      animation: msg-fade-in 0.2s ease;
    }
    @keyframes msg-fade-in {
      from { opacity: 0; transform: translateY(4px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .message-bubble.agent {
      background-color: var(--widget-bubble-agent, #f3f4f6);
      color: var(--widget-text, #1f2937);
      align-self: flex-start;
      border-bottom-left-radius: 4px;
    }
    .message-bubble.customer {
      background-color: var(--widget-bubble-customer, #1b72e8);
      color: var(--widget-bubble-customer-text, #ffffff);
      align-self: flex-end;
      margin-left: auto;
      border-bottom-right-radius: 4px;
    }
    .message-content {
      white-space: pre-wrap;
    }
    .message-time {
      font-size: 11px;
      margin-top: 4px;
      opacity: 0.7;
    }
    .message-time.agent {
      color: var(--widget-text-secondary, #6b7280);
    }
    .message-time.customer {
      color: rgba(255, 255, 255, 0.75);
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
