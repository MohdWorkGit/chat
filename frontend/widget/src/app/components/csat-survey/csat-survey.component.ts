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
      <h3 style="font-size: 18px; font-weight: 600;">
        How was your experience?
      </h3>
      <p style="color: var(--widget-text-secondary); font-size: 13px; margin-top: 4px;">
        Your feedback helps us improve our support.
      </p>

      <div class="csat-stars">
        @for (star of stars; track star) {
          <svg
            class="csat-star"
            [class.active]="star <= selectedRating()"
            (click)="selectRating(star)"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="currentColor">
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
          </svg>
        }
      </div>

      @if (selectedRating() > 0) {
        <div style="margin-top: 12px;">
          <textarea
            [(ngModel)]="feedback"
            placeholder="Any additional feedback? (optional)"
            rows="3"
            style="width: 100%; padding: 10px 12px; border: 1px solid var(--widget-border); border-radius: 8px; font-family: var(--widget-font); font-size: 14px; outline: none; resize: none;">
          </textarea>
        </div>

        <button
          style="margin-top: 12px; padding: 10px 24px; background-color: var(--widget-primary); color: white; border: none; border-radius: 8px; cursor: pointer; font-size: 14px; font-weight: 500;"
          (click)="onSubmit()">
          Submit Feedback
        </button>
      }
    </div>
  `,
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
