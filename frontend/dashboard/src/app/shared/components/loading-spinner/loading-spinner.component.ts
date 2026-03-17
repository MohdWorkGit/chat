import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center gap-3">
      <div
        class="animate-spin rounded-full border-solid border-current border-t-transparent"
        [class]="spinnerClasses"
        role="status"
        aria-label="Loading"
      ></div>
      @if (message) {
        <p class="text-sm text-slate-500">{{ message }}</p>
      }
    </div>
  `,
  styles: [`
    :host {
      display: flex;
      align-items: center;
      justify-content: center;
    }
  `],
})
export class LoadingSpinnerComponent {
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() message?: string;

  get spinnerClasses(): string {
    switch (this.size) {
      case 'sm': return 'h-5 w-5 border-2 text-indigo-600';
      case 'md': return 'h-8 w-8 border-[3px] text-indigo-600';
      case 'lg': return 'h-12 w-12 border-4 text-indigo-600';
    }
  }
}
