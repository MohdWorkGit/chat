import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { SignalRService } from '@core/services/signalr.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />'
})
export class AppComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly signalrService = inject(SignalRService);

  ngOnInit(): void {
    // When the user already has a valid token (e.g. after a page refresh),
    // the `loginSuccess$` effect never fires, so SignalR would otherwise
    // stay disconnected and the conversation view would miss every
    // real-time `message.created` event. Establish the connection here so
    // the agent receives updates immediately on load.
    if (this.authService.isAuthenticated()) {
      const accountId = this.authService.currentAccountId();
      this.signalrService.connect().then(() => {
        if (accountId) {
          this.signalrService.joinAccountGroup(accountId);
        }
      });
    }
  }
}
