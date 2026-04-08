import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface CsatData {
  rating: number;
  feedback: string;
}

@Component({
  selector: 'cew-csat-survey',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="csat-survey">
      <div class="csat-header">
        <div class="csat-icon-wrapper">
          <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm3.5 6c.83 0 1.5.67 1.5 1.5S16.33 11 15.5 11 14 10.33 14 9.5 14.67 8 15.5 8zm-7 0c.83 0 1.5.67 1.5 1.5S9.33 11 8.5 11 7 10.33 7 9.5 7.67 8 8.5 8zM12 17.5c-2.33 0-4.31-1.46-5.11-3.5h10.22c-.8 2.04-2.78 3.5-5.11 3.5z"/>
          </svg>
        </div>
        <h3 class="csat-title">How was your experience?</h3>
        <p class="csat-subtitle">Your feedback helps us improve our support.</p>
      </div>

      <div class="csat-stars" role="radiogroup" aria-label="Rate your experience">
        @for (star of stars; track star) {
          <button
            type="button"
            class="csat-star-btn"
            [class.active]="star <= selectedRating()"
            (click)="selectRating(star)"
            [attr.aria-label]="star + ' star' + (star > 1 ? 's' : '')"
            [attr.aria-pressed]="star <= selectedRating()">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              fill="currentColor">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
            </svg>
          </button>
        }
      </div>

      @if (selectedRating() > 0) {
        <div class="csat-feedback-wrapper">
          <textarea
            class="csat-feedback"
            [(ngModel)]="feedback"
            placeholder="Any additional feedback? (optional)"
            rows="3">
          </textarea>

          <button
            type="button"
            class="csat-submit"
            (click)="onSubmit()">
            Submit Feedback
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: flex;
      flex: 1;
      min-height: 0;
      font-family: var(--widget-font, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif);
    }
    .csat-survey {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 32px 24px 24px;
      overflow-y: auto;
      background: var(--widget-bg, #ffffff);
      text-align: center;
      animation: csat-fade-in 0.35s ease;
    }
    @keyframes csat-fade-in {
      from { opacity: 0; transform: translateY(8px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .csat-header {
      max-width: 280px;
    }
    .csat-icon-wrapper {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background: linear-gradient(135deg, rgba(27, 114, 232, 0.12), rgba(27, 114, 232, 0.04));
      color: var(--widget-primary, #1b72e8);
      margin-bottom: 14px;
    }
    .csat-title {
      font-size: 18px;
      font-weight: 700;
      color: var(--widget-text, #1f2937);
      margin: 0 0 6px;
      letter-spacing: -0.01em;
    }
    .csat-subtitle {
      font-size: 13px;
      color: var(--widget-text-secondary, #6b7280);
      margin: 0;
      line-height: 1.5;
    }
    .csat-stars {
      display: flex;
      justify-content: center;
      gap: 6px;
      margin: 22px 0 4px;
    }
    .csat-star-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 44px;
      height: 44px;
      padding: 4px;
      background: none;
      border: none;
      cursor: pointer;
      color: #e5e7eb;
      border-radius: 8px;
      transition: color 0.18s ease, transform 0.15s ease, background-color 0.18s ease;
    }
    .csat-star-btn svg {
      width: 100%;
      height: 100%;
      transition: filter 0.18s ease;
    }
    .csat-star-btn:hover {
      background-color: rgba(245, 158, 11, 0.08);
      transform: scale(1.08);
    }
    .csat-star-btn.active {
      color: #f59e0b;
    }
    .csat-star-btn.active svg {
      filter: drop-shadow(0 2px 4px rgba(245, 158, 11, 0.35));
    }
    .csat-feedback-wrapper {
      width: 100%;
      max-width: 320px;
      margin-top: 18px;
      display: flex;
      flex-direction: column;
      gap: 12px;
      animation: csat-feedback-in 0.28s ease;
    }
    @keyframes csat-feedback-in {
      from { opacity: 0; transform: translateY(6px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .csat-feedback {
      width: 100%;
      padding: 11px 14px;
      border: 1px solid var(--widget-border, #e5e7eb);
      border-radius: 10px;
      font-family: inherit;
      font-size: 14px;
      line-height: 1.4;
      color: var(--widget-text, #1f2937);
      background-color: #fafbfc;
      resize: vertical;
      min-height: 78px;
      max-height: 160px;
      outline: none;
      transition: border-color 0.18s ease, background-color 0.18s ease, box-shadow 0.18s ease;
      -webkit-appearance: none;
      appearance: none;
    }
    .csat-feedback::placeholder {
      color: #9ca3af;
    }
    .csat-feedback:hover {
      border-color: #d1d5db;
    }
    .csat-feedback:focus {
      border-color: var(--widget-primary, #1b72e8);
      background-color: #ffffff;
      box-shadow: 0 0 0 3px rgba(27, 114, 232, 0.12);
    }
    .csat-submit {
      padding: 12px 24px;
      background: linear-gradient(135deg, var(--widget-primary, #1b72e8) 0%, var(--widget-primary-hover, #1560c7) 100%);
      color: #ffffff;
      border: none;
      border-radius: 10px;
      font-family: inherit;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      box-shadow: 0 2px 8px rgba(27, 114, 232, 0.25);
      transition: transform 0.15s ease, box-shadow 0.18s ease;
    }
    .csat-submit:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(27, 114, 232, 0.32);
    }
    .csat-submit:active {
      transform: translateY(0);
      box-shadow: 0 2px 6px rgba(27, 114, 232, 0.25);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CsatSurveyComponent {
  @Input() conversationId = 0;
  @Output() surveySubmit = new EventEmitter<CsatData>();

  stars = [1, 2, 3, 4, 5];
  selectedRating = signal(0);
  feedback = '';

  selectRating(rating: number): void {
    this.selectedRating.set(rating);
  }

  onSubmit(): void {
    this.surveySubmit.emit({
      rating: this.selectedRating(),
      feedback: this.feedback,
    });
  }
}
