import {
  Component,
  ChangeDetectionStrategy,
  Output,
  EventEmitter,
  signal,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cew-file-upload',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="file-upload-dropzone"
      [class.dragover]="isDragOver()"
      (dragover)="onDragOver($event)"
      (dragleave)="onDragLeave($event)"
      (drop)="onDrop($event)"
      (click)="fileInput.click()">
      <input
        #fileInput
        type="file"
        style="display: none;"
        [accept]="acceptedTypes"
        (change)="onFileSelected($event)" />

      @if (selectedFile()) {
        <div class="file-preview">
          <div class="file-preview-info">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="currentColor" style="flex-shrink: 0; color: var(--widget-primary, #1b72e8);">
              <path d="M14 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm4 18H6V4h7v5h5v11z"/>
            </svg>
            <div class="file-details">
              <span class="file-name">{{ selectedFile()!.name }}</span>
              <span class="file-size">{{ formatFileSize(selectedFile()!.size) }}</span>
            </div>
          </div>
          <button
            class="file-remove-btn"
            (click)="removeFile($event)"
            aria-label="Remove file">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
            </svg>
          </button>
        </div>
      } @else {
        <div class="file-upload-placeholder">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor" style="color: var(--widget-text-secondary, #6b7280);">
            <path d="M16.5 6v11.5c0 2.21-1.79 4-4 4s-4-1.79-4-4V5c0-1.38 1.12-2.5 2.5-2.5s2.5 1.12 2.5 2.5v10.5c0 .55-.45 1-1 1s-1-.45-1-1V6H10v9.5c0 1.38 1.12 2.5 2.5 2.5s2.5-1.12 2.5-2.5V5c0-2.21-1.79-4-4-4S7 2.79 7 5v12.5c0 3.04 2.46 5.5 5.5 5.5s5.5-2.46 5.5-5.5V6h-1.5z"/>
          </svg>
          <span>Drop a file here or click to browse</span>
          <span class="file-upload-hint">Images, PDFs, documents (max 10MB)</span>
        </div>
      }
    </div>

    @if (errorMessage()) {
      <div class="file-upload-error">{{ errorMessage() }}</div>
    }
  `,
  styles: [`
    .file-upload-dropzone {
      border: 2px dashed var(--widget-border, #e5e7eb);
      border-radius: 8px;
      padding: 16px;
      cursor: pointer;
      transition: border-color 0.2s, background-color 0.2s;
      margin: 8px 16px;
    }
    .file-upload-dropzone:hover,
    .file-upload-dropzone.dragover {
      border-color: var(--widget-primary, #1b72e8);
      background-color: rgba(27, 114, 232, 0.04);
    }
    .file-upload-placeholder {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 6px;
      color: var(--widget-text-secondary, #6b7280);
      font-size: 13px;
      text-align: center;
    }
    .file-upload-hint {
      font-size: 11px;
      opacity: 0.7;
    }
    .file-preview {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 8px;
    }
    .file-preview-info {
      display: flex;
      align-items: center;
      gap: 8px;
      min-width: 0;
    }
    .file-details {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }
    .file-name {
      font-size: 13px;
      font-weight: 500;
      color: var(--widget-text, #1f2937);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .file-size {
      font-size: 11px;
      color: var(--widget-text-secondary, #6b7280);
    }
    .file-remove-btn {
      background: none;
      border: none;
      cursor: pointer;
      padding: 4px;
      color: var(--widget-text-secondary, #6b7280);
      border-radius: 4px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .file-remove-btn:hover {
      background-color: rgba(0, 0, 0, 0.06);
      color: #ef4444;
    }
    .file-upload-error {
      color: #ef4444;
      font-size: 12px;
      margin: 4px 16px 0;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FileUploadComponent {
  @Output() fileSelected = new EventEmitter<File>();

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  readonly acceptedTypes = 'image/*,.pdf,.doc,.docx,.xls,.xlsx,.txt,.csv';
  private readonly maxSizeBytes = 10 * 1024 * 1024; // 10MB

  selectedFile = signal<File | null>(null);
  isDragOver = signal(false);
  errorMessage = signal('');

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processFile(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.processFile(input.files[0]);
      input.value = '';
    }
  }

  removeFile(event: Event): void {
    event.stopPropagation();
    this.selectedFile.set(null);
    this.errorMessage.set('');
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  private processFile(file: File): void {
    this.errorMessage.set('');

    if (file.size > this.maxSizeBytes) {
      this.errorMessage.set('File size exceeds 10MB limit.');
      return;
    }

    const allowedTypes = [
      'image/', 'application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'text/plain', 'text/csv',
    ];

    const isAllowed = allowedTypes.some(type =>
      file.type.startsWith(type) || file.type === type
    );

    if (!isAllowed && file.type) {
      this.errorMessage.set('File type not supported.');
      return;
    }

    this.selectedFile.set(file);
    this.fileSelected.emit(file);
  }
}
