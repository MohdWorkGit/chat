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
  styles: [`
    .typing-indicator {
      display: flex;
      align-items: center;
      gap: 4px;
      padding: 10px 14px;
      background-color: var(--widget-bubble-agent, #f3f4f6);
      border-radius: 16px;
      border-bottom-left-radius: 4px;
      align-self: flex-start;
      width: fit-content;
    }
    .typing-dot {
      width: 7px;
      height: 7px;
      border-radius: 50%;
      background-color: var(--widget-text-secondary, #6b7280);
      animation: typing-bounce 1.4s infinite ease-in-out;
    }
    .typing-dot:nth-child(1) { animation-delay: 0s; }
    .typing-dot:nth-child(2) { animation-delay: 0.2s; }
    .typing-dot:nth-child(3) { animation-delay: 0.4s; }
    @keyframes typing-bounce {
      0%, 80%, 100% { transform: translateY(0); }
      40% { transform: translateY(-6px); }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TypingIndicatorComponent {}
