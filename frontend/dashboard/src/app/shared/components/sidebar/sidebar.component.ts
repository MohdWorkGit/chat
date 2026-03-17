import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { AvatarComponent } from '../avatar/avatar.component';
import { UserAvailability } from '@core/models/user.model';

interface NavItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, AvatarComponent],
  template: `
    <aside
      class="flex flex-col h-full bg-slate-900 text-white transition-all duration-300"
      [class.w-64]="!collapsed"
      [class.w-16]="collapsed"
    >
      <!-- Logo / Brand -->
      <div class="flex items-center h-16 px-4 border-b border-slate-700/50 shrink-0">
        @if (!collapsed) {
          <span class="text-lg font-bold text-white tracking-tight">CEP Dashboard</span>
        } @else {
          <span class="text-lg font-bold text-white mx-auto">C</span>
        }
      </div>

      <!-- Navigation -->
      <nav class="flex-1 py-4 space-y-1 px-2 overflow-y-auto">
        @for (item of navItems; track item.route) {
          <a
            [routerLink]="item.route"
            routerLinkActive="bg-slate-800 text-white"
            [routerLinkActiveOptions]="{ exact: item.route === '/conversations' }"
            class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-slate-300 hover:bg-slate-800 hover:text-white transition-colors group"
            [class.justify-center]="collapsed"
            [title]="collapsed ? item.label : ''"
          >
            <span class="text-lg shrink-0" [innerHTML]="item.icon"></span>
            @if (!collapsed) {
              <span class="text-sm font-medium truncate">{{ item.label }}</span>
            }
          </a>
        }
      </nav>

      <!-- Collapse Toggle -->
      <div class="px-2 py-2 border-t border-slate-700/50">
        <button
          (click)="toggleCollapse()"
          class="flex items-center justify-center w-full px-3 py-2 rounded-lg text-slate-400 hover:bg-slate-800 hover:text-white transition-colors"
        >
          @if (collapsed) {
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
            </svg>
          } @else {
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
            </svg>
            <span class="ml-2 text-sm">Collapse</span>
          }
        </button>
      </div>

      <!-- Account Switcher -->
      <div class="px-2 py-2 border-t border-slate-700/50">
        @if (!collapsed) {
          <div class="px-3 py-1">
            <p class="text-xs text-slate-500 uppercase tracking-wider font-medium">Account</p>
          </div>
          <button class="flex items-center gap-2 w-full px-3 py-2 rounded-lg text-slate-300 hover:bg-slate-800 transition-colors">
            <div class="h-6 w-6 rounded bg-indigo-600 flex items-center justify-center text-xs font-bold">
              {{ accountInitial }}
            </div>
            <span class="text-sm truncate">Account #{{ currentUser?.accountId }}</span>
          </button>
        }
      </div>

      <!-- User Profile -->
      <div class="px-2 py-3 border-t border-slate-700/50 shrink-0">
        <div
          class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-slate-800 transition-colors cursor-pointer"
          [class.justify-center]="collapsed"
        >
          <app-avatar
            [name]="currentUser?.name ?? 'User'"
            [avatarUrl]="currentUser?.avatar"
            size="sm"
            [showStatus]="true"
            [status]="availabilityStatus"
          />
          @if (!collapsed) {
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-white truncate">
                {{ currentUser?.displayName || currentUser?.name || 'User' }}
              </p>
              <p class="text-xs text-slate-400 capitalize">{{ currentUser?.availability || 'offline' }}</p>
            </div>
          }
        </div>
      </div>
    </aside>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class SidebarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  @Input() collapsed = false;
  @Output() collapsedChange = new EventEmitter<boolean>();

  currentUser: {
    name: string;
    displayName?: string;
    avatar?: string;
    availability: string;
    accountId: number;
  } | null = null;

  navItems: NavItem[] = [
    {
      label: 'Conversations',
      route: '/conversations',
      icon: '<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5"><path stroke-linecap="round" stroke-linejoin="round" d="M8.625 12a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0H8.25m4.125 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0H12m4.125 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0h-.375M21 12c0 4.556-4.03 8.25-9 8.25a9.764 9.764 0 01-2.555-.337A5.972 5.972 0 015.41 20.97a5.969 5.969 0 01-.474-.065 4.48 4.48 0 00.978-2.025c.09-.457-.133-.901-.467-1.226C3.93 16.178 3 14.189 3 12c0-4.556 4.03-8.25 9-8.25s9 3.694 9 8.25z" /></svg>',
    },
    {
      label: 'Contacts',
      route: '/contacts',
      icon: '<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z" /></svg>',
    },
    {
      label: 'Reports',
      route: '/reports',
      icon: '<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5"><path stroke-linecap="round" stroke-linejoin="round" d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v6.75C7.5 20.496 6.996 21 6.375 21h-2.25A1.125 1.125 0 013 19.875v-6.75zM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v11.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V8.625zM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25C20.496 3 21 3.504 21 4.125v15.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V4.125z" /></svg>',
    },
    {
      label: 'Settings',
      route: '/settings',
      icon: '<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5"><path stroke-linecap="round" stroke-linejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.324.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.24-.438.613-.431.992a6.759 6.759 0 010 .255c-.007.378.138.75.43.99l1.005.828c.424.35.534.954.26 1.43l-1.298 2.247a1.125 1.125 0 01-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.57 6.57 0 01-.22.128c-.331.183-.581.495-.644.869l-.213 1.28c-.09.543-.56.941-1.11.941h-2.594c-.55 0-1.02-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.992a6.932 6.932 0 010-.255c.007-.378-.138-.75-.43-.99l-1.004-.828a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.087.22-.128.332-.183.582-.495.644-.869l.214-1.281z" /><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /></svg>',
    },
  ];

  constructor() {
    this.authService.currentUser$.subscribe((user) => {
      if (user) {
        this.currentUser = {
          name: user.name,
          displayName: user.displayName,
          avatar: user.avatar,
          availability: user.availability,
          accountId: user.accountId,
        };
      }
    });
  }

  get availabilityStatus(): 'online' | 'busy' | 'offline' {
    const status = this.currentUser?.availability;
    if (status === 'online') return 'online';
    if (status === 'busy') return 'busy';
    return 'offline';
  }

  get accountInitial(): string {
    return 'A';
  }

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }
}
