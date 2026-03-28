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
      --widget-bubble-agent: #f3f4f6;
      --widget-bubble-customer: #1b72e8;
      --widget-bubble-customer-text: #ffffff;
      --widget-radius: 12px;
      --widget-shadow: 0 4px 24px rgba(0, 0, 0, 0.12);
      --widget-font: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      font-family: var(--widget-font);
      font-size: 14px;
      line-height: 1.5;
      color: var(--widget-text);
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
      background-color: var(--widget-primary);
      color: white;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: var(--widget-shadow);
      transition: transform 0.2s ease, background-color 0.2s ease;
      position: relative;
      flex-shrink: 0;
    }
    .widget-launcher:hover {
      transform: scale(1.05);
      background-color: var(--widget-primary-hover);
    }
    .widget-launcher svg {
      width: 28px;
      height: 28px;
      fill: currentColor;
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
