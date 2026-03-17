import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="relative inline-flex" [ngClass]="sizeClasses">
      @if (avatarUrl) {
        <img
          [src]="avatarUrl"
          [alt]="name"
          class="rounded-full object-cover"
          [ngClass]="sizeClasses"
        />
      } @else {
        <div
          class="rounded-full flex items-center justify-center font-semibold text-white"
          [ngClass]="sizeClasses"
          [style.background-color]="backgroundColor"
        >
          {{ initials }}
        </div>
      }
      @if (showStatus) {
        <span
          class="absolute bottom-0 right-0 block rounded-full ring-2 ring-white"
          [ngClass]="statusClasses"
        ></span>
      }
    </div>
  `,
  styles: [`
    :host {
      display: inline-flex;
    }
  `],
})
export class AvatarComponent {
  @Input({ required: true }) name = '';
  @Input() avatarUrl?: string;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() showStatus = false;
  @Input() status: 'online' | 'busy' | 'offline' = 'offline';

  get initials(): string {
    if (!this.name) return '?';
    const parts = this.name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0][0].toUpperCase();
  }

  get backgroundColor(): string {
    const colors = [
      '#6366f1', '#8b5cf6', '#a855f7', '#d946ef',
      '#ec4899', '#f43f5e', '#ef4444', '#f97316',
      '#eab308', '#84cc16', '#22c55e', '#14b8a6',
      '#06b6d4', '#0ea5e9', '#3b82f6', '#6366f1',
    ];
    let hash = 0;
    for (let i = 0; i < this.name.length; i++) {
      hash = this.name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
  }

  get sizeClasses(): string {
    switch (this.size) {
      case 'sm': return 'h-8 w-8 text-xs';
      case 'md': return 'h-10 w-10 text-sm';
      case 'lg': return 'h-14 w-14 text-lg';
    }
  }

  get statusClasses(): string {
    const dotSize = this.size === 'sm' ? 'h-2 w-2' : this.size === 'md' ? 'h-2.5 w-2.5' : 'h-3 w-3';
    switch (this.status) {
      case 'online': return `${dotSize} bg-green-500`;
      case 'busy': return `${dotSize} bg-yellow-500`;
      case 'offline': return `${dotSize} bg-gray-400`;
    }
  }
}
