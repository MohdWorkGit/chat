import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { Subscription, filter } from 'rxjs';
import { AuthService } from '@core/services/auth.service';
import { NotificationService } from '@core/services/notification.service';
import { AvatarComponent } from '../avatar/avatar.component';

interface Breadcrumb {
  label: string;
  route?: string;
}

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, AvatarComponent],
  template: `
    <header class="flex items-center justify-between h-16 px-6 bg-white border-b border-slate-200">
      <!-- Left: Breadcrumbs -->
      <div class="flex items-center gap-2">
        <nav class="flex items-center text-sm">
          @for (crumb of breadcrumbs(); track crumb.label; let last = $last) {
            @if (crumb.route && !last) {
              <a
                [routerLink]="crumb.route"
                class="text-slate-500 hover:text-slate-700 transition-colors"
              >
                {{ crumb.label }}
              </a>
              <svg class="h-4 w-4 text-slate-400 mx-1.5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
              </svg>
            } @else {
              <span class="text-slate-900 font-medium">{{ crumb.label }}</span>
            }
          }
        </nav>
      </div>

      <!-- Center: Search -->
      <div class="flex-1 max-w-lg mx-8">
        <div class="relative">
          <svg class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
          </svg>
          <input
            type="text"
            placeholder="Search conversations, contacts..."
            [(ngModel)]="searchQuery"
            (keydown.enter)="onSearch()"
            class="w-full pl-10 pr-4 py-2 bg-slate-100 border border-transparent rounded-lg text-sm text-slate-900 placeholder-slate-400 focus:bg-white focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 focus:outline-none transition-colors"
          />
        </div>
      </div>

      <!-- Right: Actions -->
      <div class="flex items-center gap-4">
        <!-- Notifications -->
        <div class="relative">
          <button
            (click)="toggleNotifications()"
            class="relative p-2 rounded-lg text-slate-500 hover:bg-slate-100 hover:text-slate-700 transition-colors"
          >
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M14.857 17.082a23.848 23.848 0 005.454-1.31A8.967 8.967 0 0118 9.75v-.7V9A6 6 0 006 9v.75a8.967 8.967 0 01-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 01-5.714 0m5.714 0a3 3 0 11-5.714 0" />
            </svg>
            @if (unreadCount() > 0) {
              <span class="absolute -top-0.5 -right-0.5 flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-xs font-bold text-white">
                {{ unreadCount() > 99 ? '99+' : unreadCount() }}
              </span>
            }
          </button>

          <!-- Notification Dropdown -->
          @if (showNotifications()) {
            <div class="absolute right-0 top-full mt-2 w-80 rounded-lg bg-white shadow-lg ring-1 ring-black/5 z-50">
              <div class="flex items-center justify-between p-4 border-b border-slate-100">
                <h3 class="text-sm font-semibold text-slate-900">Notifications</h3>
                <button
                  (click)="markAllRead()"
                  class="text-xs text-indigo-600 hover:text-indigo-800 font-medium"
                >
                  Mark all read
                </button>
              </div>
              <div class="max-h-64 overflow-y-auto p-2">
                <p class="text-sm text-slate-500 text-center py-6">No new notifications</p>
              </div>
            </div>
          }
        </div>

        <!-- User Menu -->
        <div class="relative">
          <button
            (click)="toggleUserMenu()"
            class="flex items-center gap-2 p-1.5 rounded-lg hover:bg-slate-100 transition-colors"
          >
            <app-avatar
              [name]="userName()"
              [avatarUrl]="userAvatar()"
              size="sm"
              [showStatus]="true"
              [status]="userStatus()"
            />
            @if (userName()) {
              <span class="text-sm font-medium text-slate-700 hidden md:block">{{ userName() }}</span>
            }
            <svg class="h-4 w-4 text-slate-400 hidden md:block" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5" />
            </svg>
          </button>

          <!-- User Dropdown -->
          @if (showUserMenu()) {
            <div class="absolute right-0 top-full mt-2 w-56 rounded-lg bg-white shadow-lg ring-1 ring-black/5 z-50">
              <div class="p-3 border-b border-slate-100">
                <p class="text-sm font-medium text-slate-900">{{ userName() }}</p>
                <p class="text-xs text-slate-500">{{ userEmail() }}</p>
              </div>
              <div class="py-1">
                <a
                  routerLink="/settings/profile"
                  (click)="showUserMenu.set(false)"
                  class="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 hover:bg-slate-50"
                >
                  <svg class="h-4 w-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" />
                  </svg>
                  Profile
                </a>

                <!-- Availability Toggle -->
                <div class="px-4 py-2">
                  <p class="text-xs text-slate-500 font-medium mb-2">Availability</p>
                  <div class="flex gap-1.5">
                    <button
                      (click)="setAvailability('online')"
                      class="flex-1 px-2 py-1 text-xs rounded font-medium transition-colors"
                      [class]="userStatus() === 'online' ? 'bg-green-100 text-green-700 ring-1 ring-green-300' : 'text-slate-500 hover:bg-slate-100'"
                    >
                      Online
                    </button>
                    <button
                      (click)="setAvailability('busy')"
                      class="flex-1 px-2 py-1 text-xs rounded font-medium transition-colors"
                      [class]="userStatus() === 'busy' ? 'bg-yellow-100 text-yellow-700 ring-1 ring-yellow-300' : 'text-slate-500 hover:bg-slate-100'"
                    >
                      Busy
                    </button>
                    <button
                      (click)="setAvailability('offline')"
                      class="flex-1 px-2 py-1 text-xs rounded font-medium transition-colors"
                      [class]="userStatus() === 'offline' ? 'bg-slate-200 text-slate-700 ring-1 ring-slate-300' : 'text-slate-500 hover:bg-slate-100'"
                    >
                      Offline
                    </button>
                  </div>
                </div>

                <div class="border-t border-slate-100 my-1"></div>

                <button
                  (click)="logout()"
                  class="flex items-center gap-2 w-full px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                >
                  <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3 0l3-3m0 0l-3-3m3 3H9" />
                  </svg>
                  Sign out
                </button>
              </div>
            </div>
          }
        </div>
      </div>
    </header>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class HeaderComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);
  private subscriptions = new Subscription();

  searchQuery = '';
  breadcrumbs = signal<Breadcrumb[]>([{ label: 'Dashboard' }]);
  unreadCount = signal(0);
  showNotifications = signal(false);
  showUserMenu = signal(false);

  userName = signal('');
  userEmail = signal('');
  userAvatar = signal<string | undefined>(undefined);
  userStatus = signal<'online' | 'busy' | 'offline'>('offline');

  ngOnInit(): void {
    this.subscriptions.add(
      this.authService.currentUser$.subscribe((user) => {
        if (user) {
          this.userName.set(user.displayName || user.name);
          this.userEmail.set(user.email);
          this.userAvatar.set(user.avatar);
          this.userStatus.set(user.availability as 'online' | 'busy' | 'offline');
        }
      })
    );

    this.subscriptions.add(
      this.notificationService.getUnreadCount().subscribe((result) => {
        this.unreadCount.set(result.count);
      })
    );

    this.subscriptions.add(
      this.router.events
        .pipe(filter((e) => e instanceof NavigationEnd))
        .subscribe(() => {
          this.updateBreadcrumbs();
          this.showNotifications.set(false);
          this.showUserMenu.set(false);
        })
    );

    this.updateBreadcrumbs();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/search'], {
        queryParams: { q: this.searchQuery.trim() },
      });
    }
  }

  toggleNotifications(): void {
    this.showNotifications.update((v) => !v);
    this.showUserMenu.set(false);
  }

  toggleUserMenu(): void {
    this.showUserMenu.update((v) => !v);
    this.showNotifications.set(false);
  }

  markAllRead(): void {
    this.notificationService.markAllAsRead().subscribe(() => {
      this.unreadCount.set(0);
    });
  }

  setAvailability(status: 'online' | 'busy' | 'offline'): void {
    this.userStatus.set(status);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  private updateBreadcrumbs(): void {
    const url = this.router.url.split('?')[0];
    const segments = url.split('/').filter(Boolean);
    const crumbs: Breadcrumb[] = [];

    const labelMap: Record<string, string> = {
      conversations: 'Conversations',
      contacts: 'Contacts',
      reports: 'Reports',
      settings: 'Settings',
      profile: 'Profile',
      search: 'Search',
    };

    let currentPath = '';
    for (const segment of segments) {
      currentPath += '/' + segment;
      const label = labelMap[segment] || segment.charAt(0).toUpperCase() + segment.slice(1);
      crumbs.push({ label, route: currentPath });
    }

    if (crumbs.length === 0) {
      crumbs.push({ label: 'Dashboard' });
    }

    this.breadcrumbs.set(crumbs);
  }
}
