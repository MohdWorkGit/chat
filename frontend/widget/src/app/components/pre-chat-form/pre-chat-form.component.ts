import { Component, ChangeDetectionStrategy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface PreChatFormData {
  name: string;
  email: string;
  customFields: Record<string, string>;
}

@Component({
  selector: 'cew-pre-chat-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="pre-chat-form">
      <div class="pre-chat-header">
        <h3 class="pre-chat-title">Start a conversation</h3>
        <p class="pre-chat-subtitle">
          We typically reply within a few minutes.
        </p>
      </div>

      <div class="pre-chat-field">
        <label for="cew-name" class="pre-chat-label">
          Name <span class="pre-chat-required">*</span>
        </label>
        <input
          id="cew-name"
          class="pre-chat-input"
          type="text"
          [(ngModel)]="formData.name"
          placeholder="Your name"
          autocomplete="name"
          required />
      </div>

      <div class="pre-chat-field">
        <label for="cew-email" class="pre-chat-label">
          Email <span class="pre-chat-required">*</span>
        </label>
        <input
          id="cew-email"
          class="pre-chat-input"
          type="email"
          [(ngModel)]="formData.email"
          placeholder="you@example.com"
          autocomplete="email"
          required />
      </div>

      <div class="pre-chat-field">
        <label for="cew-message" class="pre-chat-label">
          How can we help?
        </label>
        <textarea
          id="cew-message"
          class="pre-chat-textarea"
          [(ngModel)]="formData.customFields['initial_message']"
          placeholder="Describe your issue..."
          rows="3">
        </textarea>
      </div>

      <button
        class="pre-chat-submit"
        (click)="onSubmit()"
        [disabled]="!isValid()">
        <span>Start Chat</span>
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
          <path d="M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6z"/>
        </svg>
      </button>
    </div>
  `,
  styles: [`
    :host {
      display: flex;
      flex: 1;
      min-height: 0;
      font-family: var(--widget-font, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif);
    }
    .pre-chat-form {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 24px 20px;
      overflow-y: auto;
      background: var(--widget-bg, #ffffff);
      animation: pre-chat-fade-in 0.35s ease;
    }
    @keyframes pre-chat-fade-in {
      from { opacity: 0; transform: translateY(8px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .pre-chat-header {
      margin-bottom: 4px;
    }
    .pre-chat-title {
      font-size: 18px;
      font-weight: 700;
      color: var(--widget-text, #1f2937);
      margin: 0 0 4px;
      letter-spacing: -0.01em;
    }
    .pre-chat-subtitle {
      font-size: 13px;
      color: var(--widget-text-secondary, #6b7280);
      margin: 0;
      line-height: 1.5;
    }
    .pre-chat-field {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }
    .pre-chat-label {
      display: block;
      font-size: 12px;
      font-weight: 600;
      color: var(--widget-text, #1f2937);
      letter-spacing: 0.01em;
    }
    .pre-chat-required {
      color: #ef4444;
      margin-left: 2px;
    }
    .pre-chat-input,
    .pre-chat-textarea {
      width: 100%;
      padding: 11px 14px;
      border: 1px solid var(--widget-border, #e5e7eb);
      border-radius: 10px;
      font-family: inherit;
      font-size: 14px;
      line-height: 1.4;
      color: var(--widget-text, #1f2937);
      background-color: #fafbfc;
      outline: none;
      transition: border-color 0.18s ease, background-color 0.18s ease, box-shadow 0.18s ease;
      -webkit-appearance: none;
      appearance: none;
    }
    .pre-chat-input::placeholder,
    .pre-chat-textarea::placeholder {
      color: #9ca3af;
      opacity: 1;
    }
    .pre-chat-input:hover,
    .pre-chat-textarea:hover {
      border-color: #d1d5db;
    }
    .pre-chat-input:focus,
    .pre-chat-textarea:focus {
      border-color: var(--widget-primary, #1b72e8);
      background-color: #ffffff;
      box-shadow: 0 0 0 3px rgba(27, 114, 232, 0.12);
    }
    .pre-chat-textarea {
      resize: vertical;
      min-height: 84px;
      max-height: 180px;
      font-family: inherit;
    }
    .pre-chat-submit {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      margin-top: 4px;
      padding: 12px 20px;
      background: linear-gradient(135deg, var(--widget-primary, #1b72e8) 0%, var(--widget-primary-hover, #1560c7) 100%);
      color: #ffffff;
      border: none;
      border-radius: 10px;
      font-family: inherit;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      box-shadow: 0 2px 8px rgba(27, 114, 232, 0.25);
      transition: transform 0.15s ease, box-shadow 0.18s ease, opacity 0.18s ease;
    }
    .pre-chat-submit svg {
      transition: transform 0.18s ease;
    }
    .pre-chat-submit:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(27, 114, 232, 0.32);
    }
    .pre-chat-submit:hover:not(:disabled) svg {
      transform: translateX(2px);
    }
    .pre-chat-submit:active:not(:disabled) {
      transform: translateY(0);
      box-shadow: 0 2px 6px rgba(27, 114, 232, 0.25);
    }
    .pre-chat-submit:disabled {
      opacity: 0.55;
      cursor: not-allowed;
      box-shadow: none;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PreChatFormComponent {
  @Output() formSubmit = new EventEmitter<PreChatFormData>();

  formData: PreChatFormData = {
    name: '',
    email: '',
    customFields: { initial_message: '' },
  };

  isValid(): boolean {
    return this.formData.name.trim().length > 0
      && this.formData.email.trim().length > 0
      && this.formData.email.includes('@');
  }

  onSubmit(): void {
    if (this.isValid()) {
      this.formSubmit.emit({ ...this.formData });
    }
  }
}
