import {
  Component,
  ChangeDetectionStrategy,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cew-greeting-message',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="greeting-container">
      <div class="greeting-content">
        <div class="greeting-logo-wrapper">
          @if (teamLogoUrl) {
            <img class="greeting-logo" [src]="teamLogoUrl" [alt]="teamName" />
          } @else {
            <div class="greeting-logo-placeholder">
              <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="currentColor">
                <path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H6l-2 2V4h16v12z"/>
              </svg>
            </div>
          }
        </div>

        <h3 class="greeting-title">{{ greetingTitle }}</h3>
        <p class="greeting-text">{{ greetingText }}</p>

        <button class="greeting-cta" (click)="startConversation.emit()">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="currentColor" style="flex-shrink: 0;">
            <path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H6l-2 2V4h16v12z"/>
          </svg>
          Start a conversation
        </button>
      </div>
    </div>
  `,
  styles: [`
    .greeting-container {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 32px 24px;
      animation: greeting-fade-in 0.4s ease;
    }
    @keyframes greeting-fade-in {
      from {
        opacity: 0;
        transform: translateY(12px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    .greeting-content {
      text-align: center;
      max-width: 280px;
    }
    .greeting-logo-wrapper {
      margin-bottom: 20px;
    }
    .greeting-logo {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      object-fit: cover;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }
    .greeting-logo-placeholder {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background-color: var(--widget-primary, #1b72e8);
      color: #ffffff;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }
    .greeting-title {
      font-size: 18px;
      font-weight: 600;
      color: var(--widget-text, #1f2937);
      margin-bottom: 8px;
    }
    .greeting-text {
      font-size: 14px;
      color: var(--widget-text-secondary, #6b7280);
      line-height: 1.5;
      margin-bottom: 24px;
    }
    .greeting-cta {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 12px 24px;
      background-color: var(--widget-primary, #1b72e8);
      color: #ffffff;
      border: none;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
      transition: background-color 0.2s ease, transform 0.15s ease;
      font-family: var(--widget-font, inherit);
    }
    .greeting-cta:hover {
      background-color: var(--widget-primary-hover, #1560c7);
      transform: translateY(-1px);
    }
    .greeting-cta:active {
      transform: translateY(0);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GreetingMessageComponent {
  @Input() teamName = 'Support';
  @Input() teamLogoUrl = '';
  @Input() greetingTitle = 'Hi there!';
  @Input() greetingText = 'We are here to help. Ask us anything, or share your feedback.';
  @Output() startConversation = new EventEmitter<void>();
}
