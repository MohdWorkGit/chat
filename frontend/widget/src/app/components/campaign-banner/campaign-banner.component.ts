import {
  Component,
  ChangeDetectionStrategy,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cew-campaign-banner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="campaign-banner" (click)="bannerClick.emit()">
      <button
        class="campaign-close-btn"
        (click)="onClose($event)"
        aria-label="Dismiss banner">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor">
          <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
        </svg>
      </button>
      <div class="campaign-content">
        @if (avatarUrl) {
          <img class="campaign-avatar" [src]="avatarUrl" [alt]="senderName" />
        } @else {
          <div class="campaign-avatar-placeholder">
            {{ senderName ? senderName[0].toUpperCase() : '?' }}
          </div>
        }
        <div class="campaign-body">
          <div class="campaign-sender">{{ senderName }}</div>
          <div class="campaign-message">{{ message }}</div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .campaign-banner {
      position: absolute;
      bottom: 72px;
      right: 0;
      width: 320px;
      background: var(--widget-bg, #ffffff);
      border-radius: 12px;
      box-shadow: 0 4px 24px rgba(0, 0, 0, 0.15);
      padding: 16px;
      cursor: pointer;
      animation: campaign-slide-up 0.3s ease;
    }
    @keyframes campaign-slide-up {
      from {
        opacity: 0;
        transform: translateY(8px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    .campaign-close-btn {
      position: absolute;
      top: 8px;
      right: 8px;
      background: none;
      border: none;
      cursor: pointer;
      color: var(--widget-text-secondary, #6b7280);
      padding: 4px;
      border-radius: 4px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .campaign-close-btn:hover {
      background-color: rgba(0, 0, 0, 0.06);
    }
    .campaign-content {
      display: flex;
      gap: 12px;
      align-items: flex-start;
    }
    .campaign-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      object-fit: cover;
      flex-shrink: 0;
    }
    .campaign-avatar-placeholder {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background-color: var(--widget-primary, #1b72e8);
      color: #ffffff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 14px;
      flex-shrink: 0;
    }
    .campaign-body {
      flex: 1;
      min-width: 0;
    }
    .campaign-sender {
      font-weight: 600;
      font-size: 13px;
      color: var(--widget-text, #1f2937);
      margin-bottom: 2px;
    }
    .campaign-message {
      font-size: 13px;
      color: var(--widget-text-secondary, #6b7280);
      line-height: 1.4;
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CampaignBannerComponent {
  @Input() message = '';
  @Input() senderName = '';
  @Input() avatarUrl = '';

  @Output() bannerClick = new EventEmitter<void>();
  @Output() bannerClose = new EventEmitter<void>();

  onClose(event: Event): void {
    event.stopPropagation();
    this.bannerClose.emit();
  }
}
