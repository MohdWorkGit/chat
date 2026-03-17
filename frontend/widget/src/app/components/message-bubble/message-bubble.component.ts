import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  selector: 'cew-message-bubble',
  standalone: true,
  imports: [CommonModule, DatePipe],
  template: `
    <div class="message-bubble" [ngClass]="senderType">
      <div [innerHTML]="content"></div>
      <div class="message-time" [ngClass]="senderType === 'customer' ? 'text-right' : ''">
        {{ timestamp | date:'shortTime' }}
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageBubbleComponent {
  @Input() content = '';
  @Input() senderType: 'agent' | 'customer' = 'agent';
  @Input() timestamp: string | Date = new Date();
}
