import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { CannedResponsesActions } from '@store/canned-responses/canned-responses.actions';

@Component({
  selector: 'app-canned-response-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-2xl mx-auto px-6 py-6">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <div>
            <a routerLink="/settings/canned-responses" class="text-sm text-blue-600 hover:text-blue-500 flex items-center gap-1 mb-2">
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
              </svg>
              Back to Canned Responses
            </a>
            <h1 class="text-xl font-semibold text-gray-900">New Canned Response</h1>
          </div>
        </div>

        <!-- Form -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <form [formGroup]="responseForm" (ngSubmit)="onSubmit()" class="space-y-5">
            <!-- Short Code -->
            <div>
              <label class="block text-sm font-medium text-gray-700">Short Code <span class="text-red-500">*</span></label>
              <div class="mt-1 flex items-center">
                <span class="inline-flex items-center rounded-l-lg border border-r-0 border-gray-300 bg-gray-50 px-3 py-2 text-sm text-gray-500">/</span>
                <input
                  formControlName="shortCode"
                  class="block w-full rounded-r-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="greeting"
                />
              </div>
              @if (responseForm.get('shortCode')?.hasError('required') && responseForm.get('shortCode')?.touched) {
                <p class="mt-1 text-xs text-red-500">Short code is required</p>
              }
              <p class="mt-1 text-xs text-gray-400">Type "/" followed by the short code to quickly insert this response.</p>
            </div>

            <!-- Content -->
            <div>
              <label class="block text-sm font-medium text-gray-700">Content <span class="text-red-500">*</span></label>
              <textarea
                formControlName="content"
                rows="6"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Type the canned response content..."
              ></textarea>
              @if (responseForm.get('content')?.hasError('required') && responseForm.get('content')?.touched) {
                <p class="mt-1 text-xs text-red-500">Content is required</p>
              }
            </div>

            <!-- Preview -->
            @if (responseForm.get('content')?.value) {
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Preview</label>
                <div class="bg-gray-50 rounded-lg border border-gray-200 p-4">
                  <p class="text-sm text-gray-700 whitespace-pre-wrap">{{ responseForm.get('content')?.value }}</p>
                </div>
              </div>
            }

            <!-- Actions -->
            <div class="flex items-center gap-3 pt-4 border-t border-gray-200">
              <button
                type="submit"
                [disabled]="responseForm.invalid"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                Create Canned Response
              </button>
              <button
                type="button"
                (click)="onCancel()"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class CannedResponseFormComponent {
  private store = inject(Store);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  responseForm: FormGroup = this.fb.group({
    shortCode: ['', Validators.required],
    content: ['', Validators.required],
  });

  onSubmit(): void {
    if (this.responseForm.invalid) return;
    this.store.dispatch(
      CannedResponsesActions.createCannedResponse({ data: this.responseForm.value })
    );
    this.router.navigate(['/settings/canned-responses']);
  }

  onCancel(): void {
    this.router.navigate(['/settings/canned-responses']);
  }
}
