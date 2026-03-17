import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'cew-typing-indicator',
  standalone: true,
  template: `
    <div class="typing-indicator">
      <span class="typing-dot"></span>
      <span class="typing-dot"></span>
      <span class="typing-dot"></span>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TypingIndicatorComponent {}
