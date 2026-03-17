import {
  Component,
  Output,
  EventEmitter,
  ViewChild,
  ElementRef,
  AfterViewInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-reply-box',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="border-t border-gray-200 bg-white">
      <!-- Mode Tabs -->
      <div class="flex border-b border-gray-200">
        <button
          (click)="setMode('reply')"
          class="px-4 py-2 text-sm font-medium transition-colors"
          [class]="mode === 'reply'
            ? 'text-blue-600 border-b-2 border-blue-600'
            : 'text-gray-500 hover:text-gray-700'"
        >
          Reply
        </button>
        <button
          (click)="setMode('note')"
          class="px-4 py-2 text-sm font-medium transition-colors"
          [class]="mode === 'note'
            ? 'text-yellow-600 border-b-2 border-yellow-600'
            : 'text-gray-500 hover:text-gray-700'"
        >
          Private Note
        </button>
      </div>

      <!-- Canned response dropdown -->
      @if (showCannedDropdown && filteredCanned.length > 0) {
        <div class="absolute bottom-full left-0 right-0 bg-white border border-gray-200 rounded-t-lg shadow-lg max-h-40 overflow-y-auto z-10 mx-3 mb-1">
          @for (canned of filteredCanned; track canned.shortCode) {
            <button
              (click)="insertCanned(canned)"
              class="w-full text-left px-3 py-2 text-sm hover:bg-gray-50 border-b border-gray-50 last:border-0"
            >
              <span class="font-medium text-gray-700">/{{ canned.shortCode }}</span>
              <span class="text-gray-400 ml-2">{{ canned.content | slice:0:60 }}</span>
            </button>
          }
        </div>
      }

      <!-- Input area -->
      <div class="relative p-3" [class.bg-yellow-50]="mode === 'note'">
        <textarea
          #textareaRef
          [(ngModel)]="content"
          (input)="onInput()"
          (keydown)="onKeyDown($event)"
          placeholder="{{ mode === 'note' ? 'Write a private note...' : 'Type your reply...' }}"
          rows="3"
          class="w-full resize-none rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
          [class.border-yellow-300]="mode === 'note'"
          [class.focus:ring-yellow-500]="mode === 'note'"
        ></textarea>

        <!-- Actions -->
        <div class="flex items-center justify-between mt-2">
          <div class="flex items-center gap-2">
            <!-- Attachment button -->
            <button
              type="button"
              class="p-1.5 text-gray-400 hover:text-gray-600 rounded transition-colors"
              title="Attach file"
            >
              <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m18.375 12.739-7.693 7.693a4.5 4.5 0 0 1-6.364-6.364l10.94-10.94A3 3 0 1 1 19.5 7.372L8.552 18.32m.009-.01-.01.01m5.699-9.941-7.81 7.81a1.5 1.5 0 0 0 2.112 2.13" />
              </svg>
            </button>

            <span class="text-xs text-gray-400">
              Type / for canned responses. Ctrl+Enter to send.
            </span>
          </div>

          <button
            (click)="send()"
            [disabled]="!content.trim()"
            class="px-4 py-1.5 text-sm font-medium text-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            [class]="mode === 'note'
              ? 'bg-yellow-500 hover:bg-yellow-600'
              : 'bg-blue-600 hover:bg-blue-700'"
          >
            @if (mode === 'note') {
              Add Note
            } @else {
              Send
            }
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      position: relative;
    }
  `],
})
export class ReplyBoxComponent implements AfterViewInit, OnDestroy {
  @Output() messageSent = new EventEmitter<{ content: string; isPrivate: boolean }>();
  @ViewChild('textareaRef') textareaRef!: ElementRef<HTMLTextAreaElement>;

  content = '';
  mode: 'reply' | 'note' = 'reply';
  showCannedDropdown = false;
  cannedSearchQuery = '';

  // Placeholder canned responses for shortcode detection
  cannedResponses = [
    { shortCode: 'greeting', content: 'Hello! Thank you for reaching out. How can I help you today?' },
    { shortCode: 'thanks', content: 'Thank you for your patience. Is there anything else I can help with?' },
    { shortCode: 'resolved', content: 'I\'m glad we could resolve this. Feel free to reach out if you need anything else.' },
    { shortCode: 'followup', content: 'I\'m following up on your previous inquiry. Have you had a chance to try the solution?' },
  ];

  filteredCanned = this.cannedResponses;

  private keydownHandler = this.handleGlobalKeydown.bind(this);

  ngAfterViewInit(): void {
    document.addEventListener('keydown', this.keydownHandler);
  }

  ngOnDestroy(): void {
    document.removeEventListener('keydown', this.keydownHandler);
  }

  setMode(mode: 'reply' | 'note'): void {
    this.mode = mode;
  }

  onInput(): void {
    this.autoResize();
    this.detectCannedShortcode();
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) {
      event.preventDefault();
      this.send();
    }

    if (event.key === 'Escape') {
      this.showCannedDropdown = false;
    }
  }

  private handleGlobalKeydown(event: KeyboardEvent): void {
    // No-op for global shortcuts beyond what onKeyDown handles
  }

  private autoResize(): void {
    const textarea = this.textareaRef?.nativeElement;
    if (textarea) {
      textarea.style.height = 'auto';
      textarea.style.height = Math.min(textarea.scrollHeight, 200) + 'px';
    }
  }

  private detectCannedShortcode(): void {
    const match = this.content.match(/\/(\w*)$/);
    if (match) {
      this.cannedSearchQuery = match[1];
      this.filteredCanned = this.cannedResponses.filter((c) =>
        c.shortCode.toLowerCase().includes(this.cannedSearchQuery.toLowerCase()),
      );
      this.showCannedDropdown = true;
    } else {
      this.showCannedDropdown = false;
    }
  }

  insertCanned(canned: { shortCode: string; content: string }): void {
    this.content = this.content.replace(/\/\w*$/, canned.content);
    this.showCannedDropdown = false;
    this.textareaRef?.nativeElement.focus();
  }

  send(): void {
    const trimmed = this.content.trim();
    if (!trimmed) return;

    this.messageSent.emit({
      content: trimmed,
      isPrivate: this.mode === 'note',
    });

    this.content = '';
    this.autoResize();
  }
}
