import {
  Component,
  ChangeDetectionStrategy,
  Input,
} from '@angular/core';
import { CommonModule } from '@angular/common';

export type AgentAvailabilityStatus = 'online' | 'busy' | 'offline';

export interface AgentInfo {
  name: string;
  avatarUrl: string;
  status: AgentAvailabilityStatus;
  replyTimeMinutes: number;
}

@Component({
  selector: 'cew-agent-availability',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="agent-availability">
      <div class="agent-availability-avatar-wrapper">
        @if (avatarUrl) {
          <img
            class="agent-availability-avatar"
            [src]="avatarUrl"
            [alt]="name" />
        } @else {
          <div class="agent-availability-avatar-placeholder">
            {{ getInitials() }}
          </div>
        }
        <span
          class="agent-availability-indicator"
          [class.online]="status === 'online'"
          [class.busy]="status === 'busy'"
          [class.offline]="status === 'offline'"
          [attr.aria-label]="status + ' status'">
        </span>
      </div>
      <div class="agent-availability-info">
        <div class="agent-availability-name">{{ name }}</div>
        <div class="agent-availability-reply-time">{{ getReplyTimeText() }}</div>
      </div>
    </div>
  `,
  styles: [`
    .agent-availability {
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .agent-availability-avatar-wrapper {
      position: relative;
      flex-shrink: 0;
    }
    .agent-availability-avatar,
    .agent-availability-avatar-placeholder {
      width: 36px;
      height: 36px;
      border-radius: 50%;
    }
    .agent-availability-avatar {
      object-fit: cover;
    }
    .agent-availability-avatar-placeholder {
      background-color: rgba(255, 255, 255, 0.2);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 14px;
      color: inherit;
    }
    .agent-availability-indicator {
      position: absolute;
      bottom: 0;
      right: 0;
      width: 10px;
      height: 10px;
      border-radius: 50%;
      border: 2px solid var(--widget-primary, #1b72e8);
    }
    .agent-availability-indicator.online {
      background-color: #22c55e;
    }
    .agent-availability-indicator.busy {
      background-color: #eab308;
    }
    .agent-availability-indicator.offline {
      background-color: #9ca3af;
    }
    .agent-availability-info {
      min-width: 0;
    }
    .agent-availability-name {
      font-weight: 600;
      font-size: 15px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .agent-availability-reply-time {
      font-size: 12px;
      opacity: 0.85;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AgentAvailabilityComponent {
  @Input() name = 'Support';
  @Input() avatarUrl = '';
  @Input() status: AgentAvailabilityStatus = 'offline';
  @Input() replyTimeMinutes = 5;

  getInitials(): string {
    return this.name
      .split(' ')
      .map(n => n[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  getReplyTimeText(): string {
    if (this.status === 'online') {
      return 'Available now';
    }
    if (this.replyTimeMinutes < 60) {
      return `Typically replies in ${this.replyTimeMinutes} min`;
    }
    const hours = Math.floor(this.replyTimeMinutes / 60);
    return `Typically replies in ${hours}h`;
  }
}
