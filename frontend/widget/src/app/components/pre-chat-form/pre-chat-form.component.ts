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
      <h3 style="font-size: 18px; font-weight: 600; margin-bottom: 4px;">
        Start a conversation
      </h3>
      <p style="color: var(--widget-text-secondary); font-size: 13px;">
        We typically reply within a few minutes.
      </p>

      <div>
        <label for="name" style="display: block; font-size: 13px; font-weight: 500; margin-bottom: 4px;">
          Name <span style="color: #ef4444;">*</span>
        </label>
        <input
          id="name"
          type="text"
          [(ngModel)]="formData.name"
          placeholder="Your name"
          required />
      </div>

      <div>
        <label for="email" style="display: block; font-size: 13px; font-weight: 500; margin-bottom: 4px;">
          Email <span style="color: #ef4444;">*</span>
        </label>
        <input
          id="email"
          type="email"
          [(ngModel)]="formData.email"
          placeholder="you@example.com"
          required />
      </div>

      <div>
        <label for="message" style="display: block; font-size: 13px; font-weight: 500; margin-bottom: 4px;">
          How can we help?
        </label>
        <textarea
          id="message"
          [(ngModel)]="formData.customFields['initial_message']"
          placeholder="Describe your issue..."
          rows="3">
        </textarea>
      </div>

      <button
        (click)="onSubmit()"
        [disabled]="!isValid()">
        Start Chat
      </button>
    </div>
  `,
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
