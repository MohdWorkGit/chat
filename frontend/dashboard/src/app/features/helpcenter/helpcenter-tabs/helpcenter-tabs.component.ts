import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-helpcenter-tabs',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="flex gap-1 border-b border-gray-200 px-6 pt-4 bg-white">
      <a
        routerLink="/helpcenter/articles"
        routerLinkActive="border-blue-600 text-blue-600"
        [routerLinkActiveOptions]="{ exact: false }"
        class="px-4 py-2 text-sm font-medium text-gray-600 border-b-2 border-transparent hover:text-gray-900"
      >
        Articles
      </a>
      <a
        routerLink="/helpcenter/categories"
        routerLinkActive="border-blue-600 text-blue-600"
        [routerLinkActiveOptions]="{ exact: false }"
        class="px-4 py-2 text-sm font-medium text-gray-600 border-b-2 border-transparent hover:text-gray-900"
      >
        Categories
      </a>
    </nav>
  `,
})
export class HelpCenterTabsComponent {}
