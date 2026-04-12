import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';

@Component({
  selector: 'app-email-template-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6 max-w-3xl">
      <div class="mb-6">
        <button (click)="goBack()" class="text-sm text-blue-600 hover:text-blue-800 mb-2 inline-flex items-center gap-1">
          <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
          </svg>
          Back to Email Templates
        </button>
        <h2 class="text-lg font-semibold text-gray-900">{{ isEditing ? 'Edit Template' : 'New Template' }}</h2>
      </div>

      <div class="bg-white rounded-lg border border-gray-200 p-6">
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-5">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Template Name <span class="text-red-500">*</span></label>
            <input
              formControlName="name"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="e.g. Welcome Email"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Template Type</label>
            <select
              formControlName="templateType"
              class="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
            >
              <option value="">— Select type —</option>
              <option value="welcome">Welcome</option>
              <option value="password_reset">Password Reset</option>
              <option value="email_confirmation">Email Confirmation</option>
              <option value="csat">CSAT Survey</option>
              <option value="conversation_reply">Conversation Reply</option>
              <option value="notification">Notification</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Locale</label>
            <input
              formControlName="locale"
              class="w-40 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
              placeholder="en"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Body
              <span class="ml-1 text-xs text-gray-400 font-normal">Use &#123;&#123;variable&#125;&#125; for dynamic values</span>
            </label>
            <textarea
              formControlName="body"
              rows="14"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm font-mono focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="<html>...</html>"
            ></textarea>
          </div>

          @if (error) {
            <div class="rounded-lg bg-red-50 border border-red-200 px-4 py-3">
              <p class="text-sm text-red-700">{{ error }}</p>
            </div>
          }

          <div class="flex items-center gap-3 pt-2">
            <button
              type="submit"
              [disabled]="form.invalid || saving"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-lg transition-colors"
            >
              {{ saving ? 'Saving...' : (isEditing ? 'Save Changes' : 'Create Template') }}
            </button>
            <button type="button" (click)="goBack()" class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [':host { display: block; height: 100%; overflow-y: auto; }'],
})
export class EmailTemplateFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  isEditing = false;
  saving = false;
  error: string | null = null;
  private templateId: number | null = null;

  form: FormGroup = this.fb.group({
    name: ['', Validators.required],
    templateType: [''],
    locale: ['en'],
    body: [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditing = true;
      this.templateId = +id;
      this.api.get<any>(this.api.accountPath(`/email_templates/${id}`)).subscribe({
        next: (tpl) => this.form.patchValue({ name: tpl.name, templateType: tpl.templateType ?? '', locale: tpl.locale ?? 'en', body: tpl.body ?? '' }),
        error: () => { this.error = 'Failed to load template.'; },
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = null;
    const payload = this.form.value;

    const req$ = this.isEditing && this.templateId
      ? this.api.put(this.api.accountPath(`/email_templates/${this.templateId}`), payload)
      : this.api.post(this.api.accountPath('/email_templates'), payload);

    req$.subscribe({
      next: () => { this.saving = false; this.router.navigate(['/settings/email-templates']); },
      error: () => { this.saving = false; this.error = 'Failed to save template.'; },
    });
  }

  goBack(): void {
    this.router.navigate(['/settings/email-templates']);
  }
}
