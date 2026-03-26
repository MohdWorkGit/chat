import {
  Component,
  ChangeDetectionStrategy,
  Input,
  computed,
  signal,
  OnChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cew-unread-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (badgeCount() > 0) {
      <span class="unread-badge" aria-label="Unread messages">
        {{ displayText() }}
      </span>
    }
  `,
  styles: [`
    .unread-badge {
      position: absolute;
      top: -4px;
      right: -4px;
      min-width: 20px;
      height: 20px;
      padding: 0 6px;
      border-radius: 10px;
      background-color: #ef4444;
      color: #ffffff;
      font-size: 11px;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      line-height: 1;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      animation: badge-pop 0.3s ease;
    }
    @keyframes badge-pop {
      0% { transform: scale(0); }
      50% { transform: scale(1.2); }
      100% { transform: scale(1); }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UnreadBadgeComponent implements OnChanges {
  @Input() count = 0;

  badgeCount = signal(0);

  displayText = computed(() => {
    const c = this.badgeCount();
    return c > 9 ? '9+' : String(c);
  });

  ngOnChanges(): void {
    this.badgeCount.set(this.count);
  }
}
