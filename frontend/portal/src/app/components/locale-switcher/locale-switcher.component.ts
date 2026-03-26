import { Component, ChangeDetectionStrategy, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface Locale {
  code: string;
  label: string;
}

const AVAILABLE_LOCALES: Locale[] = [
  { code: 'en', label: 'English' },
  { code: 'es', label: 'Spanish' },
  { code: 'fr', label: 'French' },
  { code: 'de', label: 'German' },
  { code: 'ar', label: 'Arabic' },
];

@Component({
  selector: 'portal-locale-switcher',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="portal-locale-switcher">
      <button
        class="portal-locale-switcher-toggle"
        (click)="toggleDropdown()"
        [attr.aria-expanded]="isOpen()"
        aria-haspopup="listbox">
        <span>{{ currentLocaleLabel() }}</span>
        <span class="portal-locale-switcher-arrow" [class.open]="isOpen()">&#9662;</span>
      </button>
      @if (isOpen()) {
        <ul class="portal-locale-switcher-dropdown" role="listbox">
          @for (locale of locales; track locale.code) {
            <li
              role="option"
              [class.active]="locale.code === currentLocale()"
              (click)="selectLocale(locale)">
              {{ locale.label }}
            </li>
          }
        </ul>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LocaleSwitcherComponent {
  @Output() localeChange = new EventEmitter<string>();

  locales = AVAILABLE_LOCALES;
  currentLocale = signal(this.getStoredLocale());
  currentLocaleLabel = signal(this.getLabelForCode(this.getStoredLocale()));
  isOpen = signal(false);

  toggleDropdown(): void {
    this.isOpen.update(v => !v);
  }

  selectLocale(locale: Locale): void {
    this.currentLocale.set(locale.code);
    this.currentLocaleLabel.set(locale.label);
    this.isOpen.set(false);
    localStorage.setItem('portal-locale', locale.code);
    this.localeChange.emit(locale.code);
  }

  private getStoredLocale(): string {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem('portal-locale') || 'en';
    }
    return 'en';
  }

  private getLabelForCode(code: string): string {
    return AVAILABLE_LOCALES.find(l => l.code === code)?.label || 'English';
  }
}
