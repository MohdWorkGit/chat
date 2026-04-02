import { Component, inject, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '@core/services/api.service';

interface CsvRow {
  [key: string]: string;
}

interface FieldMapping {
  csvColumn: string;
  contactField: string;
}

@Component({
  selector: 'app-contact-import',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-3xl mx-auto px-6 py-6">
        <!-- Header -->
        <div class="mb-6">
          <a routerLink="/contacts" class="text-sm text-blue-600 hover:text-blue-500 flex items-center gap-1 mb-2">
            <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
            </svg>
            Back to Contacts
          </a>
          <h1 class="text-xl font-semibold text-gray-900">Import Contacts</h1>
          <p class="text-sm text-gray-500 mt-1">Upload a CSV file to import contacts in bulk.</p>
        </div>

        <!-- Step 1: File Upload -->
        <div class="bg-white rounded-lg border border-gray-200 mb-6">
          <div class="px-4 py-3 border-b border-gray-200">
            <h3 class="text-sm font-semibold text-gray-900 flex items-center gap-2">
              <span class="h-5 w-5 rounded-full bg-blue-600 text-white text-xs flex items-center justify-center font-bold">1</span>
              Upload CSV File
            </h3>
          </div>
          <div class="p-4">
            @if (!csvFile) {
              <div
                (click)="fileInput.click()"
                (dragover)="onDragOver($event)"
                (dragleave)="isDragOver = false"
                (drop)="onDrop($event)"
                class="border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors"
                [class]="isDragOver ? 'border-blue-400 bg-blue-50' : 'border-gray-300 hover:border-gray-400'"
              >
                <svg class="mx-auto h-10 w-10 text-gray-400 mb-3" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M3 16.5v2.25A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75V16.5m-13.5-9L12 3m0 0 4.5 4.5M12 3v13.5" />
                </svg>
                <p class="text-sm font-medium text-gray-700">Drop your CSV file here or click to browse</p>
                <p class="text-xs text-gray-400 mt-1">Supports .csv files up to 10MB</p>
              </div>
              <input
                #fileInput
                type="file"
                accept=".csv"
                (change)="onFileSelected($event)"
                class="hidden"
              />
            } @else {
              <div class="flex items-center justify-between bg-gray-50 rounded-lg p-3">
                <div class="flex items-center gap-3">
                  <svg class="h-8 w-8 text-green-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z" />
                  </svg>
                  <div>
                    <p class="text-sm font-medium text-gray-900">{{ csvFile.name }}</p>
                    <p class="text-xs text-gray-500">{{ csvHeaders.length }} columns, {{ csvData.length }} rows detected</p>
                  </div>
                </div>
                <button
                  (click)="removeFile()"
                  class="text-sm text-red-500 hover:text-red-700 transition-colors"
                >
                  Remove
                </button>
              </div>
            }
            @if (parseError) {
              <div class="mt-3 p-3 bg-red-50 border border-red-200 rounded-lg">
                <p class="text-xs text-red-700">{{ parseError }}</p>
              </div>
            }
          </div>
        </div>

        <!-- Step 2: Field Mapping -->
        @if (csvHeaders.length > 0) {
          <div class="bg-white rounded-lg border border-gray-200 mb-6">
            <div class="px-4 py-3 border-b border-gray-200">
              <h3 class="text-sm font-semibold text-gray-900 flex items-center gap-2">
                <span class="h-5 w-5 rounded-full bg-blue-600 text-white text-xs flex items-center justify-center font-bold">2</span>
                Map Fields
              </h3>
              <p class="text-xs text-gray-500 mt-1">Match your CSV columns to contact fields</p>
            </div>
            <div class="p-4">
              <div class="space-y-3">
                @for (mapping of fieldMappings; track $index) {
                  <div class="flex items-center gap-3">
                    <div class="flex-1">
                      <label class="block text-xs font-medium text-gray-500 mb-1">CSV Column</label>
                      <div class="px-3 py-2 bg-gray-50 rounded-lg border border-gray-200 text-sm text-gray-700">
                        {{ mapping.csvColumn }}
                      </div>
                    </div>
                    <div class="flex items-center pt-5">
                      <svg class="h-5 w-5 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M13.5 4.5 21 12m0 0-7.5 7.5M21 12H3" />
                      </svg>
                    </div>
                    <div class="flex-1">
                      <label class="block text-xs font-medium text-gray-500 mb-1">Contact Field</label>
                      <select
                        [(ngModel)]="mapping.contactField"
                        class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      >
                        <option value="">-- Skip this column --</option>
                        @for (field of availableContactFields; track field.value) {
                          <option [value]="field.value">{{ field.label }}</option>
                        }
                      </select>
                    </div>
                  </div>
                }
              </div>
            </div>
          </div>
        }

        <!-- Step 3: Preview -->
        @if (csvHeaders.length > 0 && hasMappedFields) {
          <div class="bg-white rounded-lg border border-gray-200 mb-6">
            <div class="px-4 py-3 border-b border-gray-200">
              <h3 class="text-sm font-semibold text-gray-900 flex items-center gap-2">
                <span class="h-5 w-5 rounded-full bg-blue-600 text-white text-xs flex items-center justify-center font-bold">3</span>
                Preview
              </h3>
              <p class="text-xs text-gray-500 mt-1">First {{ previewRows.length }} rows of your import</p>
            </div>
            <div class="p-4 overflow-x-auto">
              <table class="min-w-full text-sm">
                <thead>
                  <tr class="border-b border-gray-200">
                    @for (mapping of activeMappings; track mapping.csvColumn) {
                      <th class="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        {{ getFieldLabel(mapping.contactField) }}
                      </th>
                    }
                  </tr>
                </thead>
                <tbody>
                  @for (row of previewRows; track $index) {
                    <tr class="border-b border-gray-100">
                      @for (mapping of activeMappings; track mapping.csvColumn) {
                        <td class="px-3 py-2 text-gray-700">
                          {{ row[mapping.csvColumn] || '-' }}
                        </td>
                      }
                    </tr>
                  }
                </tbody>
              </table>
              @if (csvData.length > 5) {
                <p class="text-xs text-gray-400 mt-2 text-center">
                  ...and {{ csvData.length - 5 }} more rows
                </p>
              }
            </div>
          </div>

          <!-- Import Action -->
          <div class="flex items-center gap-4">
            <button
              (click)="startImport()"
              [disabled]="importing"
              class="px-6 py-2.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
            >
              @if (importing) {
                <svg class="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                </svg>
                Importing...
              } @else {
                Import {{ csvData.length }} Contacts
              }
            </button>
            @if (importing) {
              <div class="flex-1">
                <div class="flex items-center justify-between text-xs text-gray-500 mb-1">
                  <span>Progress</span>
                  <span>{{ importProgress }}%</span>
                </div>
                <div class="w-full bg-gray-200 rounded-full h-2">
                  <div
                    class="bg-blue-600 h-2 rounded-full transition-all duration-300"
                    [style.width.%]="importProgress"
                  ></div>
                </div>
              </div>
            }
          </div>
        }

        <!-- Import Complete -->
        @if (importComplete) {
          <div class="mt-6 p-4 bg-green-50 border border-green-200 rounded-lg">
            <div class="flex items-center gap-2 mb-2">
              <svg class="h-5 w-5 text-green-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
              </svg>
              <p class="text-sm font-medium text-green-800">Import completed!</p>
            </div>
            <p class="text-xs text-green-700 ml-7">
              Successfully imported {{ importedCount }} contacts.
              @if (failedCount > 0) {
                <span class="text-orange-700">{{ failedCount }} rows failed to import.</span>
              }
            </p>
            <a routerLink="/contacts" class="mt-2 inline-block ml-7 text-sm text-green-700 hover:text-green-600 underline">
              View contacts
            </a>
          </div>
        }

        <!-- Import Error -->
        @if (importError) {
          <div class="mt-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p class="text-sm text-red-700">{{ importError }}</p>
          </div>
        }
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
export class ContactImportComponent {
  private readonly api = inject(ApiService);

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  csvFile: File | null = null;
  csvHeaders: string[] = [];
  csvData: CsvRow[] = [];
  fieldMappings: FieldMapping[] = [];
  parseError = '';
  isDragOver = false;

  importing = false;
  importProgress = 0;
  importComplete = false;
  importedCount = 0;
  failedCount = 0;
  importError = '';

  availableContactFields = [
    { value: 'name', label: 'Name' },
    { value: 'email', label: 'Email' },
    { value: 'phone', label: 'Phone' },
    { value: 'contactType', label: 'Contact Type' },
    { value: 'company', label: 'Company' },
    { value: 'location', label: 'Location' },
    { value: 'identifier', label: 'Identifier' },
  ];

  get hasMappedFields(): boolean {
    return this.fieldMappings.some((m) => m.contactField !== '');
  }

  get activeMappings(): FieldMapping[] {
    return this.fieldMappings.filter((m) => m.contactField !== '');
  }

  get previewRows(): CsvRow[] {
    return this.csvData.slice(0, 5);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processFile(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.processFile(input.files[0]);
    }
  }

  removeFile(): void {
    this.csvFile = null;
    this.csvHeaders = [];
    this.csvData = [];
    this.fieldMappings = [];
    this.parseError = '';
    this.importComplete = false;
    this.importError = '';
  }

  private processFile(file: File): void {
    if (!file.name.endsWith('.csv')) {
      this.parseError = 'Please upload a valid CSV file.';
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      this.parseError = 'File size exceeds 10MB limit.';
      return;
    }

    this.parseError = '';
    this.csvFile = file;

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const text = e.target?.result as string;
        this.parseCsv(text);
      } catch {
        this.parseError = 'Failed to parse CSV file. Please check the file format.';
      }
    };
    reader.readAsText(file);
  }

  private parseCsv(text: string): void {
    const lines = text.split('\n').filter((line) => line.trim());
    if (lines.length < 2) {
      this.parseError = 'CSV file must have a header row and at least one data row.';
      return;
    }

    this.csvHeaders = this.parseCsvLine(lines[0]);
    this.csvData = [];

    for (let i = 1; i < lines.length; i++) {
      const values = this.parseCsvLine(lines[i]);
      const row: CsvRow = {};
      this.csvHeaders.forEach((header, idx) => {
        row[header] = values[idx] || '';
      });
      this.csvData.push(row);
    }

    // Auto-map fields by matching header names
    this.fieldMappings = this.csvHeaders.map((header) => {
      const normalizedHeader = header.toLowerCase().trim().replace(/[_\s-]/g, '');
      let matchedField = '';

      for (const field of this.availableContactFields) {
        const normalizedField = field.value.toLowerCase();
        const normalizedLabel = field.label.toLowerCase().replace(/[_\s-]/g, '');
        if (normalizedHeader === normalizedField || normalizedHeader === normalizedLabel) {
          matchedField = field.value;
          break;
        }
      }

      // Common aliases
      if (!matchedField) {
        if (normalizedHeader.includes('firstname') || normalizedHeader.includes('fullname')) matchedField = 'name';
        else if (normalizedHeader.includes('email') || normalizedHeader.includes('mail')) matchedField = 'email';
        else if (normalizedHeader.includes('phone') || normalizedHeader.includes('mobile') || normalizedHeader.includes('tel')) matchedField = 'phone';
        else if (normalizedHeader.includes('company') || normalizedHeader.includes('org')) matchedField = 'company';
        else if (normalizedHeader.includes('city') || normalizedHeader.includes('location') || normalizedHeader.includes('country')) matchedField = 'location';
      }

      return { csvColumn: header, contactField: matchedField };
    });
  }

  private parseCsvLine(line: string): string[] {
    const result: string[] = [];
    let current = '';
    let inQuotes = false;

    for (let i = 0; i < line.length; i++) {
      const char = line[i];
      if (char === '"') {
        inQuotes = !inQuotes;
      } else if (char === ',' && !inQuotes) {
        result.push(current.trim());
        current = '';
      } else {
        current += char;
      }
    }
    result.push(current.trim());
    return result;
  }

  getFieldLabel(fieldValue: string): string {
    return this.availableContactFields.find((f) => f.value === fieldValue)?.label || fieldValue;
  }

  startImport(): void {
    if (!this.hasMappedFields || this.importing) return;

    this.importing = true;
    this.importProgress = 0;
    this.importError = '';
    this.importComplete = false;

    const mappedData = this.csvData.map((row) => {
      const contact: Record<string, unknown> = {};
      for (const mapping of this.activeMappings) {
        const value = row[mapping.csvColumn];
        if (value) {
          if (mapping.contactField === 'company') {
            contact['company'] = { name: value };
          } else {
            contact[mapping.contactField] = value;
          }
        }
      }
      return contact;
    });

    const formData = new FormData();
    formData.append('mappings', JSON.stringify(this.activeMappings));
    if (this.csvFile) {
      formData.append('file', this.csvFile);
    }

    // Simulate progress while the upload happens
    const progressInterval = setInterval(() => {
      if (this.importProgress < 90) {
        this.importProgress += Math.random() * 15;
        if (this.importProgress > 90) this.importProgress = 90;
      }
    }, 500);

    this.api.upload<{ imported: number; failed: number }>(
      this.api.accountPath('/contacts/import'),
      formData
    ).subscribe({
      next: (result) => {
        clearInterval(progressInterval);
        this.importProgress = 100;
        this.importing = false;
        this.importComplete = true;
        this.importedCount = result.imported;
        this.failedCount = result.failed;
      },
      error: (err) => {
        clearInterval(progressInterval);
        this.importing = false;
        this.importError = err?.error?.message || 'Import failed. Please try again.';
      },
    });
  }
}
