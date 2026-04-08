import { Component, ChangeDetectionStrategy, Input, OnInit, OnDestroy, ViewEncapsulation, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { ChatWindowComponent } from './components/chat-window/chat-window.component';
import { UnreadBadgeComponent } from './components/unread-badge/unread-badge.component';
import { CampaignBannerComponent } from './components/campaign-banner/campaign-banner.component';
import { SignalrService, CampaignMessage } from './services/signalr.service';

@Component({
  selector: 'cew-root',
  standalone: true,
  imports: [CommonModule, ChatWindowComponent, UnreadBadgeComponent, CampaignBannerComponent],
  template: `
    <div class="widget-container">
      @if (isChatOpen) {
        <cew-chat-window
          [websiteToken]="websiteToken"
          [locale]="locale"
          (close)="toggleChat()" />
      }

      @if (!isChatOpen && campaignMessage()) {
        <cew-campaign-banner
          [message]="campaignMessage()!.message"
          [senderName]="campaignMessage()!.senderName"
          [avatarUrl]="campaignMessage()!.avatarUrl"
          (bannerClick)="onCampaignBannerClick()"
          (bannerClose)="dismissCampaignBanner()" />
      }

      <button
        class="widget-launcher"
        (click)="toggleChat()"
        [attr.aria-label]="isChatOpen ? 'Close chat' : 'Open chat'">
        @if (!isChatOpen) {
          <cew-unread-badge [count]="unreadCount()" />
        }
        @if (isChatOpen) {
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
            <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
          </svg>
        } @else {
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
            <path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H6l-2 2V4h16v12z"/>
          </svg>
        }
      </button>
    </div>
  `,
  styles: [`
    :host {
      --widget-primary: #1b72e8;
      --widget-primary-hover: #1560c7;
      --widget-bg: #ffffff;
      --widget-text: #1f2937;
      --widget-text-secondary: #6b7280;
      --widget-border: #e5e7eb;
      --widget-bubble-agent: #ffffff;
      --widget-bubble-customer: #1b72e8;
      --widget-bubble-customer-text: #ffffff;
      --widget-radius: 16px;
      --widget-shadow: 0 12px 40px rgba(17, 24, 39, 0.18), 0 2px 10px rgba(17, 24, 39, 0.06);
      --widget-font: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
      font-family: var(--widget-font);
      font-size: 14px;
      line-height: 1.5;
      color: var(--widget-text);
      -webkit-font-smoothing: antialiased;
      -moz-osx-font-smoothing: grayscale;
      text-rendering: optimizeLegibility;
    }
    *, *::before, *::after {
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }
    .widget-container {
      position: fixed;
      bottom: 20px;
      right: 20px;
      z-index: 999999;
      font-family: var(--widget-font);
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 12px;
    }
    .widget-launcher {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--widget-primary) 0%, var(--widget-primary-hover) 100%);
      color: #ffffff;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 8px 24px rgba(27, 114, 232, 0.35), 0 2px 6px rgba(17, 24, 39, 0.08);
      transition: transform 0.2s cubic-bezier(0.16, 1, 0.3, 1), box-shadow 0.22s ease;
      position: relative;
      flex-shrink: 0;
      font-family: inherit;
      -webkit-tap-highlight-color: transparent;
    }
    .widget-launcher::before {
      content: '';
      position: absolute;
      inset: 0;
      border-radius: 50%;
      background: radial-gradient(circle at 30% 30%, rgba(255, 255, 255, 0.22), transparent 55%);
      pointer-events: none;
    }
    .widget-launcher:hover {
      transform: translateY(-2px) scale(1.04);
      box-shadow: 0 12px 28px rgba(27, 114, 232, 0.42), 0 4px 10px rgba(17, 24, 39, 0.1);
    }
    .widget-launcher:active {
      transform: translateY(0) scale(0.98);
    }
    .widget-launcher svg {
      width: 28px;
      height: 28px;
      fill: currentColor;
      position: relative;
      z-index: 1;
      transition: transform 0.25s ease;
    }
    .widget-launcher:hover svg {
      transform: scale(1.08);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.ShadowDom,
})
export class AppComponent implements OnInit, OnDestroy {
  @Input() websiteToken = '';
  @Input() locale = 'en';

  isChatOpen = false;
  unreadCount = signal(0);
  campaignMessage = signal<CampaignMessage | null>(null);

  private readonly destroy$ = new Subject<void>();

  constructor(private readonly signalrService: SignalrService) {}

  ngOnInit(): void {
    if (this.websiteToken) {
      this.signalrService.initialize(this.websiteToken);
    }

    this.signalrService.messages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (!this.isChatOpen) {
          this.unreadCount.update(c => c + 1);
        }
      });

    this.signalrService.campaignMessage$
      .pipe(takeUntil(this.destroy$))
      .subscribe((msg: CampaignMessage) => {
        if (!this.isChatOpen) {
          this.campaignMessage.set(msg);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalrService.disconnect();
  }

  toggleChat(): void {
    this.isChatOpen = !this.isChatOpen;
    if (this.isChatOpen) {
      this.unreadCount.set(0);
      this.campaignMessage.set(null);
    }
  }

  onCampaignBannerClick(): void {
    this.campaignMessage.set(null);
    this.isChatOpen = true;
    this.unreadCount.set(0);
  }

  dismissCampaignBanner(): void {
    this.campaignMessage.set(null);
  }
}
