import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

export interface BreadcrumbItem {
  label: string;
  url?: string;
}

@Component({
  selector: 'portal-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <nav class="portal-breadcrumb">
      @for (item of items; track item.label; let last = $last) {
        @if (!last && item.url) {
          <a [routerLink]="item.url">{{ item.label }}</a>
        } @else {
          <span [class.portal-breadcrumb-current]="last">{{ item.label }}</span>
        }
        @if (!last) {
          <span class="portal-breadcrumb-separator">&gt;</span>
        }
      }
    </nav>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BreadcrumbComponent {
  @Input() items: BreadcrumbItem[] = [];
}
