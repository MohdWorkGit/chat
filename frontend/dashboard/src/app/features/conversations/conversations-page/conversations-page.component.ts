import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConversationListComponent } from '../conversation-list/conversation-list.component';

@Component({
  selector: 'app-conversations-page',
  standalone: true,
  imports: [RouterOutlet, ConversationListComponent],
  template: `
    <div class="flex h-full w-full">
      <app-conversation-list />
      <router-outlet />
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
      width: 100%;
    }
  `],
})
export class ConversationsPageComponent {}
