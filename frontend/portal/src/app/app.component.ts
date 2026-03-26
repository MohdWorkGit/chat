import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LocaleSwitcherComponent } from './components/locale-switcher/locale-switcher.component';
import { FolderNavigationComponent } from './components/folder-navigation/folder-navigation.component';

@Component({
  selector: 'portal-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, LocaleSwitcherComponent, FolderNavigationComponent],
  template: `
    <header class="portal-header">
      <div class="container portal-header-inner">
        <a routerLink="/" class="portal-logo">
          <span style="font-weight: 700; font-size: 1.125rem; color: var(--portal-text);">Help Center</span>
        </a>
        <button
          class="portal-hamburger"
          (click)="toggleMobileMenu()"
          [attr.aria-expanded]="isMobileMenuOpen()"
          aria-label="Toggle navigation menu">
          @if (isMobileMenuOpen()) {
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
            </svg>
          } @else {
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
              <path d="M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z"/>
            </svg>
          }
        </button>
        <nav class="portal-nav" [class.portal-nav-open]="isMobileMenuOpen()">
          <a routerLink="/" style="font-size: 0.875rem; color: var(--portal-text-secondary);" (click)="closeMobileMenu()">Home</a>
          <a routerLink="/search" style="font-size: 0.875rem; color: var(--portal-text-secondary);" (click)="closeMobileMenu()">Search</a>
          <portal-locale-switcher (localeChange)="onLocaleChange($event)" />
        </nav>
      </div>
    </header>

    <div class="portal-layout">
      <aside class="portal-sidebar" [class.portal-sidebar-open]="isMobileMenuOpen()">
        <portal-folder-navigation />
      </aside>
      <main class="portal-main">
        <router-outlet />
      </main>
    </div>

    @if (isMobileMenuOpen()) {
      <div class="portal-overlay" (click)="closeMobileMenu()"></div>
    }

    <footer style="border-top: 1px solid var(--portal-border); padding: 32px 0; margin-top: 64px; text-align: center;">
      <div class="container">
        <p style="color: var(--portal-text-secondary); font-size: 0.875rem;">
          Powered by Customer Engagement Platform
        </p>
      </div>
    </footer>
  `,
  styles: [`
    .portal-header {
      background: var(--portal-surface);
      border-bottom: 1px solid var(--portal-border);
      position: sticky;
      top: 0;
      z-index: 100;
    }
    .portal-header-inner {
      display: flex;
      align-items: center;
      justify-content: space-between;
      height: 64px;
    }
    .portal-logo {
      text-decoration: none;
    }
    .portal-logo:hover {
      text-decoration: none;
    }
    .portal-nav {
      display: flex;
      align-items: center;
      gap: 24px;
    }
    .portal-hamburger {
      display: none;
      background: none;
      border: none;
      cursor: pointer;
      padding: 4px;
      color: var(--portal-text);
    }
    .portal-layout {
      display: flex;
      max-width: var(--portal-max-width);
      margin: 0 auto;
      padding: 0 24px;
    }
    .portal-sidebar {
      width: 260px;
      flex-shrink: 0;
      border-right: 1px solid var(--portal-border);
      min-height: calc(100vh - 64px);
    }
    .portal-main {
      flex: 1;
      min-width: 0;
    }
    .portal-overlay {
      display: none;
    }

    @media (max-width: 768px) {
      .portal-hamburger {
        display: flex;
        align-items: center;
        justify-content: center;
      }
      .portal-nav {
        display: none;
        position: absolute;
        top: 64px;
        left: 0;
        right: 0;
        background: var(--portal-surface);
        border-bottom: 1px solid var(--portal-border);
        flex-direction: column;
        padding: 16px 24px;
        gap: 16px;
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
        z-index: 101;
      }
      .portal-nav-open {
        display: flex;
      }
      .portal-sidebar {
        display: none;
        position: fixed;
        top: 64px;
        left: 0;
        bottom: 0;
        width: 280px;
        background: var(--portal-surface);
        z-index: 200;
        border-right: 1px solid var(--portal-border);
        overflow-y: auto;
      }
      .portal-sidebar-open {
        display: block;
      }
      .portal-overlay {
        display: block;
        position: fixed;
        top: 64px;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.3);
        z-index: 199;
      }
      .portal-layout {
        padding: 0 16px;
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  isMobileMenuOpen = signal(false);

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update(v => !v);
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }

  onLocaleChange(locale: string): void {
    // Locale change handled - can be used to reload content in the future
    console.log('Locale changed to:', locale);
  }
}
