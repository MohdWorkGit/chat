import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center py-12 px-4 text-center">
      @if (icon) {
        <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-slate-100 text-slate-400 mb-4">
          <span class="text-3xl">{{ icon }}</span>
        </div>
      } @else {
        <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-slate-100 mb-4">
          <svg class="h-8 w-8 text-slate-400" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M20.25 7.5l-.625 10.632a2.25 2.25 0 01-2.247 2.118H6.622a2.25 2.25 0 01-2.247-2.118L3.75 7.5m6 4.125l2.25 2.25m0 0l2.25 2.25M12 11.625l2.25-2.25M12 11.625l-2.25 2.25M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125z" />
          </svg>
        </div>
      }
      <h3 class="text-sm font-semibold text-slate-900">{{ title }}</h3>
      @if (message) {
        <p class="mt-1 text-sm text-slate-500 max-w-sm">{{ message }}</p>
      }
      <div class="mt-6">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class EmptyStateComponent {
  @Input({ required: true }) title = '';
  @Input() message?: string;
  @Input() icon?: string;
}
