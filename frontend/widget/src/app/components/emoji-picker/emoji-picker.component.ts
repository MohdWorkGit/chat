import {
  Component,
  ChangeDetectionStrategy,
  Output,
  EventEmitter,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface EmojiCategory {
  name: string;
  emojis: string[];
}

@Component({
  selector: 'cew-emoji-picker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="emoji-picker">
      <div class="emoji-search">
        <input
          type="text"
          placeholder="Search emoji..."
          [(ngModel)]="searchQuery"
          (ngModelChange)="onSearchChange($event)"
          aria-label="Search emojis" />
      </div>
      <div class="emoji-grid-container">
        @for (category of filteredCategories(); track category.name) {
          <div class="emoji-category-label">{{ category.name }}</div>
          <div class="emoji-grid">
            @for (emoji of category.emojis; track emoji) {
              <button
                class="emoji-btn"
                (click)="selectEmoji(emoji)"
                [attr.aria-label]="'Emoji ' + emoji">
                {{ emoji }}
              </button>
            }
          </div>
        }
        @if (filteredCategories().length === 0) {
          <div class="emoji-no-results">No emojis found</div>
        }
      </div>
    </div>
  `,
  styles: [`
    .emoji-picker {
      position: absolute;
      bottom: 100%;
      left: 0;
      right: 0;
      background: var(--widget-bg, #ffffff);
      border: 1px solid var(--widget-border, #e5e7eb);
      border-radius: 12px;
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12);
      margin-bottom: 8px;
      max-height: 280px;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      z-index: 10;
    }
    .emoji-search {
      padding: 8px;
      border-bottom: 1px solid var(--widget-border, #e5e7eb);
    }
    .emoji-search input {
      width: 100%;
      padding: 6px 10px;
      border: 1px solid var(--widget-border, #e5e7eb);
      border-radius: 6px;
      font-size: 13px;
      outline: none;
      font-family: inherit;
    }
    .emoji-search input:focus {
      border-color: var(--widget-primary, #1b72e8);
    }
    .emoji-grid-container {
      overflow-y: auto;
      padding: 4px 8px 8px;
    }
    .emoji-category-label {
      font-size: 11px;
      font-weight: 600;
      color: var(--widget-text-secondary, #6b7280);
      text-transform: uppercase;
      letter-spacing: 0.5px;
      padding: 6px 4px 2px;
    }
    .emoji-grid {
      display: grid;
      grid-template-columns: repeat(8, 1fr);
      gap: 2px;
    }
    .emoji-btn {
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: none;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-size: 18px;
      line-height: 1;
      transition: background-color 0.15s;
    }
    .emoji-btn:hover {
      background-color: var(--widget-bubble-agent, #f3f4f6);
    }
    .emoji-no-results {
      text-align: center;
      padding: 20px;
      color: var(--widget-text-secondary, #6b7280);
      font-size: 13px;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmojiPickerComponent {
  @Output() emojiSelected = new EventEmitter<string>();

  searchQuery = '';
  searchFilter = signal('');

  private readonly categories: EmojiCategory[] = [
    {
      name: 'Smileys',
      emojis: ['😀', '😃', '😄', '😁', '😅', '😂', '🤣', '😊', '😇', '🙂', '😉', '😌', '😍', '🥰', '😘', '😗', '😙', '😚', '😋', '😛', '😜', '🤪', '😝', '🤗'],
    },
    {
      name: 'Gestures',
      emojis: ['👍', '👎', '👌', '✌️', '🤞', '🤟', '🤘', '🤙', '👋', '🤚', '🖐️', '✋', '👏', '🙌', '🤝', '🙏'],
    },
    {
      name: 'Hearts',
      emojis: ['❤️', '🧡', '💛', '💚', '💙', '💜', '🖤', '🤍', '💔', '💕', '💞', '💓', '💗', '💖', '💘', '💝'],
    },
    {
      name: 'Objects',
      emojis: ['🔥', '⭐', '💡', '📎', '📌', '✅', '❌', '⚡', '🎉', '🎊', '💬', '👀', '🚀', '💪', '🏆', '🔔'],
    },
  ];

  filteredCategories = computed(() => {
    const query = this.searchFilter().toLowerCase();
    if (!query) return this.categories;

    return this.categories
      .map(category => ({
        name: category.name,
        emojis: category.emojis.filter(() => {
          // Simple filter: match category name against search
          return category.name.toLowerCase().includes(query);
        }),
      }))
      .filter(category => category.emojis.length > 0);
  });

  onSearchChange(value: string): void {
    this.searchFilter.set(value);
  }

  selectEmoji(emoji: string): void {
    this.emojiSelected.emit(emoji);
  }
}
