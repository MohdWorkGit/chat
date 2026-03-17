import { Component, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from '@shared/components/sidebar/sidebar.component';
import { HeaderComponent } from '@shared/components/header/header.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, HeaderComponent],
  template: `
    <div class="flex h-screen overflow-hidden bg-slate-50">
      <!-- Mobile Overlay -->
      @if (mobileMenuOpen() && isMobile()) {
        <div
          class="fixed inset-0 z-40 bg-black/50 lg:hidden"
          (click)="mobileMenuOpen.set(false)"
        ></div>
      }

      <!-- Sidebar -->
      <div
        class="shrink-0 z-50"
        [class]="sidebarContainerClasses()"
      >
        <app-sidebar
          [collapsed]="sidebarCollapsed()"
          (collapsedChange)="onSidebarCollapsedChange($event)"
        />
      </div>

      <!-- Main Content -->
      <div class="flex flex-1 flex-col overflow-hidden">
        <!-- Mobile Header Toggle -->
        <div class="lg:hidden flex items-center h-16 px-4 bg-white border-b border-slate-200">
          <button
            (click)="mobileMenuOpen.set(true)"
            class="p-2 rounded-lg text-slate-500 hover:bg-slate-100"
          >
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
            </svg>
          </button>
          <span class="ml-3 text-lg font-semibold text-slate-900">CEP Dashboard</span>
        </div>

        <!-- Desktop Header -->
        <div class="hidden lg:block">
          <app-header />
        </div>

        <!-- Router Content -->
        <main class="flex-1 overflow-y-auto">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
    }
  `],
})
export class MainLayoutComponent {
  sidebarCollapsed = signal(false);
  mobileMenuOpen = signal(false);
  screenWidth = signal(typeof window !== 'undefined' ? window.innerWidth : 1024);

  @HostListener('window:resize', ['$event'])
  onResize(): void {
    this.screenWidth.set(window.innerWidth);
    if (this.screenWidth() >= 1024) {
      this.mobileMenuOpen.set(false);
    }
  }

  isMobile(): boolean {
    return this.screenWidth() < 1024;
  }

  sidebarContainerClasses(): string {
    if (this.isMobile()) {
      return this.mobileMenuOpen()
        ? 'fixed inset-y-0 left-0 z-50 transition-transform duration-300 transform translate-x-0'
        : 'fixed inset-y-0 left-0 z-50 transition-transform duration-300 transform -translate-x-full';
    }
    return 'relative h-full';
  }

  onSidebarCollapsedChange(collapsed: boolean): void {
    this.sidebarCollapsed.set(collapsed);
  }
}
