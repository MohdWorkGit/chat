import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'portal-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <header class="portal-header">
      <div class="container" style="display: flex; align-items: center; justify-content: space-between; height: 64px;">
        <a routerLink="/" class="portal-logo">
          <span style="font-weight: 700; font-size: 1.125rem; color: var(--portal-text);">Help Center</span>
        </a>
        <nav style="display: flex; align-items: center; gap: 24px;">
          <a routerLink="/" style="font-size: 0.875rem; color: var(--portal-text-secondary);">Home</a>
          <a routerLink="/search" style="font-size: 0.875rem; color: var(--portal-text-secondary);">Search</a>
        </nav>
      </div>
    </header>

    <main>
      <router-outlet />
    </main>

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
    .portal-logo {
      text-decoration: none;
    }
    .portal-logo:hover {
      text-decoration: none;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {}
