import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { HelpCenterService } from '@core/services/helpcenter.service';
import { HelpCenterTabsComponent } from '../helpcenter-tabs/helpcenter-tabs.component';

@Component({
  selector: 'app-portal-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HelpCenterTabsComponent],
  template: `
    <app-helpcenter-tabs />
    <div class="p-6 max-w-3xl mx-auto">
      <div class="mb-6">
        <button
          (click)="goBack()"
          class="text-sm text-blue-600 hover:text-blue-800 mb-2 inline-flex items-center gap-1"
        >
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
          </svg>
          Back to Portals
        </button>
        <h2 class="text-lg font-semibold text-gray-900">{{ isEditing ? 'Edit Portal' : 'New Portal' }}</h2>
      </div>

      <div class="bg-white rounded-lg border border-gray-200 p-6">
        <form [formGroup]="portalForm" (ngSubmit)="onSubmit()" class="space-y-5">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Name <span class="text-red-500">*</span></label>
            <input
              formControlName="name"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Acme Help Center"
            />
            @if (portalForm.get('name')?.invalid && portalForm.get('name')?.touched) {
              <p class="mt-1 text-xs text-red-600">Name is required.</p>
            }
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Slug</label>
            <input
              formControlName="slug"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="auto-generated from name"
            />
            <p class="mt-1 text-xs text-gray-500">URL identifier. Leave empty to generate from name.</p>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Custom Domain</label>
              <input
                formControlName="customDomain"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="help.example.com"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Theme Color</label>
              <input
                formControlName="color"
                type="color"
                class="h-10 w-full rounded-lg border border-gray-300 px-1 py-1"
              />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Page Title</label>
            <input
              formControlName="pageTitle"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Header Text</label>
            <input
              formControlName="headerText"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Homepage Link</label>
            <input
              formControlName="homepageLink"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="https://example.com"
            />
          </div>

          @if (isEditing) {
            <div class="pt-2 border-t border-gray-200">
              <label class="block text-sm font-medium text-gray-700 mb-2">Portal Logo</label>
              <div class="flex items-center gap-4">
                <input
                  type="file"
                  accept="image/*"
                  (change)="onFileSelected($event)"
                  class="text-sm"
                />
                <button
                  type="button"
                  (click)="uploadLogo()"
                  [disabled]="!selectedFile || uploadingLogo"
                  class="px-3 py-1.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded"
                >
                  {{ uploadingLogo ? 'Uploading...' : 'Upload' }}
                </button>
              </div>
            </div>
          }

          @if (error) {
            <div class="rounded-lg bg-red-50 border border-red-200 px-4 py-3">
              <p class="text-sm text-red-700">{{ error }}</p>
            </div>
          }

          <div class="flex items-center gap-3 pt-2">
            <button
              type="submit"
              [disabled]="portalForm.invalid || saving"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-lg transition-colors"
            >
              {{ saving ? 'Saving...' : (isEditing ? 'Save Changes' : 'Create Portal') }}
            </button>
            <button
              type="button"
              (click)="goBack()"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg transition-colors"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; height: 100%; overflow-y: auto; }
  `],
})
export class PortalFormComponent implements OnInit {
  private readonly helpCenter = inject(HelpCenterService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  isEditing = false;
  saving = false;
  uploadingLogo = false;
  error: string | null = null;
  selectedFile: File | null = null;

  private portalId: number | null = null;

  portalForm: FormGroup = this.fb.group({
    name: ['', [Validators.required]],
    slug: [''],
    customDomain: [''],
    color: ['#2563eb'],
    pageTitle: [''],
    headerText: [''],
    homepageLink: [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditing = true;
      this.portalId = +id;
      this.loadPortal();
    }
  }

  private loadPortal(): void {
    if (!this.portalId) return;
    this.helpCenter.getPortal(this.portalId).subscribe({
      next: (portal) => {
        this.portalForm.patchValue({
          name: portal.name,
          slug: portal.slug,
          customDomain: portal.customDomain ?? '',
          color: portal.color || '#2563eb',
          pageTitle: portal.pageTitle ?? '',
          headerText: portal.headerText ?? '',
          homepageLink: portal.homepageLink ?? '',
        });
      },
      error: () => {
        this.error = 'Failed to load portal data.';
      },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  uploadLogo(): void {
    if (!this.portalId || !this.selectedFile) return;
    this.uploadingLogo = true;
    this.helpCenter.uploadPortalLogo(this.portalId, this.selectedFile).subscribe({
      next: () => {
        this.uploadingLogo = false;
        this.selectedFile = null;
      },
      error: () => {
        this.uploadingLogo = false;
        this.error = 'Logo upload failed.';
      },
    });
  }

  onSubmit(): void {
    if (this.portalForm.invalid) return;

    this.saving = true;
    this.error = null;
    const payload = this.portalForm.value;

    const request$: Observable<unknown> = this.isEditing && this.portalId
      ? this.helpCenter.updatePortal(this.portalId, payload)
      : this.helpCenter.createPortal(payload);

    request$.subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/helpcenter/portals']);
      },
      error: () => {
        this.saving = false;
        this.error = 'Failed to save portal. Please try again.';
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/helpcenter/portals']);
  }
}
