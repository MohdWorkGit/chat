import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CaptainService, CaptainDocument, CaptainAssistant } from '@core/services/captain.service';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-document-manager',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center gap-3">
          <a routerLink="/captain" class="text-gray-400 hover:text-gray-600 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
            </svg>
          </a>
          <div>
            <h2 class="text-lg font-semibold text-gray-900">Documents</h2>
            @if (assistant) {
              <p class="text-xs text-gray-500">{{ assistant.name }}</p>
            }
          </div>
        </div>
      </div>

      <!-- Loading State -->
      @if (loading) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        <!-- Upload Area -->
        <div
          class="mb-6 border-2 border-dashed rounded-lg p-8 text-center transition-colors"
          [class.border-blue-400]="isDragging"
          [class.bg-blue-50]="isDragging"
          [class.border-gray-300]="!isDragging"
          (dragover)="onDragOver($event)"
          (dragleave)="onDragLeave($event)"
          (drop)="onDrop($event)"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="mx-auto h-10 w-10 text-gray-400 mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5" />
          </svg>
          <p class="text-sm text-gray-600 mb-1">Drag and drop files here, or</p>
          <label class="inline-block cursor-pointer">
            <span class="text-sm font-medium text-blue-600 hover:text-blue-800">browse files</span>
            <input
              type="file"
              class="hidden"
              (change)="onFileSelected($event)"
              multiple
            />
          </label>
          @if (uploading) {
            <div class="mt-3">
              <div class="h-1.5 w-48 mx-auto bg-gray-200 rounded-full overflow-hidden">
                <div class="h-full bg-blue-600 rounded-full animate-pulse" style="width: 60%"></div>
              </div>
              <p class="text-xs text-gray-500 mt-1">Uploading...</p>
            </div>
          }
        </div>

        <!-- Documents List -->
        @if (documents.length > 0) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">File Name</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Content Type</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Uploaded</th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                @for (doc of documents; track doc.id) {
                  <tr class="hover:bg-gray-50 transition-colors">
                    <td class="px-6 py-4">
                      <div class="flex items-center gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z" />
                        </svg>
                        <span class="text-sm text-gray-900">{{ doc.fileName }}</span>
                      </div>
                    </td>
                    <td class="px-6 py-4 text-sm text-gray-500">{{ doc.contentType }}</td>
                    <td class="px-6 py-4">
                      @if (doc.processedAt) {
                        <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                          <span class="h-1.5 w-1.5 rounded-full bg-green-500"></span>
                          Processed
                        </span>
                      } @else {
                        <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                          <span class="h-1.5 w-1.5 rounded-full bg-yellow-500 animate-pulse"></span>
                          Pending
                        </span>
                      }
                    </td>
                    <td class="px-6 py-4 text-sm text-gray-500">{{ doc.createdAt | date:'short' }}</td>
                    <td class="px-6 py-4 text-right">
                      <button
                        (click)="deleteDocument(doc)"
                        class="text-sm text-red-600 hover:text-red-800"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        } @else {
          <div class="bg-white rounded-lg border border-gray-200 text-center py-8">
            <p class="text-sm text-gray-500">No documents uploaded yet.</p>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class DocumentManagerComponent implements OnInit {
  private captainService = inject(CaptainService);
  private route = inject(ActivatedRoute);
  private auth = inject(AuthService);

  documents: CaptainDocument[] = [];
  assistant: CaptainAssistant | null = null;
  loading = true;
  uploading = false;
  isDragging = false;

  private get accountId(): number {
    return this.auth.currentAccountId();
  }
  private assistantId = 0;

  ngOnInit(): void {
    this.assistantId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadAssistant();
    this.loadDocuments();
  }

  loadAssistant(): void {
    this.captainService.getAssistant(this.accountId, this.assistantId).subscribe({
      next: (assistant) => {
        this.assistant = assistant;
      },
    });
  }

  loadDocuments(): void {
    this.loading = true;
    this.captainService.getDocuments(this.accountId, this.assistantId).subscribe({
      next: (documents) => {
        this.documents = documents;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    const files = event.dataTransfer?.files;
    if (files) {
      this.uploadFiles(Array.from(files));
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.uploadFiles(Array.from(input.files));
      input.value = '';
    }
  }

  private uploadFiles(files: File[]): void {
    this.uploading = true;
    let completed = 0;

    for (const file of files) {
      this.captainService
        .uploadDocument(this.accountId, this.assistantId, file)
        .subscribe({
          next: () => {
            completed++;
            if (completed === files.length) {
              this.uploading = false;
              this.loadDocuments();
            }
          },
          error: () => {
            completed++;
            if (completed === files.length) {
              this.uploading = false;
              this.loadDocuments();
            }
          },
        });
    }
  }

  deleteDocument(doc: CaptainDocument): void {
    if (confirm(`Are you sure you want to delete "${doc.fileName}"?`)) {
      this.captainService
        .deleteDocument(this.accountId, this.assistantId, doc.id)
        .subscribe({
          next: () => this.loadDocuments(),
        });
    }
  }
}
