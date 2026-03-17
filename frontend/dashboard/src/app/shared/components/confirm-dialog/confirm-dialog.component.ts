import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  template: `
    <div class="fixed inset-0 z-50 overflow-y-auto">
      <!-- Backdrop -->
      <div
        class="fixed inset-0 bg-black/50 transition-opacity"
        (click)="onCancel()"
      ></div>

      <!-- Dialog -->
      <div class="flex min-h-full items-center justify-center p-4">
        <div class="relative w-full max-w-md transform rounded-lg bg-white p-6 shadow-xl transition-all">
          <!-- Icon -->
          <div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full" [class]="iconBgClass">
            @if (variant === 'danger') {
              <svg class="h-6 w-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
              </svg>
            } @else if (variant === 'warning') {
              <svg class="h-6 w-6 text-yellow-600" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
              </svg>
            } @else {
              <svg class="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z" />
              </svg>
            }
          </div>

          <!-- Content -->
          <div class="mt-3 text-center">
            <h3 class="text-lg font-semibold text-slate-900">{{ title }}</h3>
            <p class="mt-2 text-sm text-slate-500">{{ message }}</p>
          </div>

          <!-- Actions -->
          <div class="mt-5 flex gap-3 justify-end">
            <button
              (click)="onCancel()"
              class="inline-flex justify-center rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
            >
              {{ cancelLabel }}
            </button>
            <button
              (click)="onConfirm()"
              class="inline-flex justify-center rounded-md px-4 py-2 text-sm font-medium text-white shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2"
              [class]="confirmButtonClass"
            >
              {{ confirmLabel }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: contents;
    }
  `],
})
export class ConfirmDialogComponent {
  @Input() title = 'Confirm Action';
  @Input() message = 'Are you sure you want to proceed?';
  @Input() confirmLabel = 'Confirm';
  @Input() cancelLabel = 'Cancel';
  @Input() variant: 'danger' | 'warning' | 'info' = 'info';
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  get iconBgClass(): string {
    switch (this.variant) {
      case 'danger': return 'bg-red-100';
      case 'warning': return 'bg-yellow-100';
      case 'info': return 'bg-blue-100';
    }
  }

  get confirmButtonClass(): string {
    switch (this.variant) {
      case 'danger': return 'bg-red-600 hover:bg-red-700 focus:ring-red-500';
      case 'warning': return 'bg-yellow-600 hover:bg-yellow-700 focus:ring-yellow-500';
      case 'info': return 'bg-indigo-600 hover:bg-indigo-700 focus:ring-indigo-500';
    }
  }

  onConfirm(): void {
    this.confirmed.emit();
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}
