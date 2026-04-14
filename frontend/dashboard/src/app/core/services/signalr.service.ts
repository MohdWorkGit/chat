import { Injectable, inject, OnDestroy } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '@env/environment';
import { AuthService } from './auth.service';
import { Message } from '@core/models/conversation.model';

export interface TypingEvent {
  conversationId: number;
  userId: number;
  userName: string;
  isTyping: boolean;
}

export interface PresenceEvent {
  userId: number;
  status: 'online' | 'offline' | 'busy';
}

@Injectable({
  providedIn: 'root',
})
export class SignalRService implements OnDestroy {
  private readonly authService = inject(AuthService);
  private connection: signalR.HubConnection | null = null;
  // Tracks conversation groups the client wants to be in. `pendingJoins`
  // holds IDs that were requested before the connection was ready (or while
  // it was reconnecting); `joinedConversations` holds IDs the server has
  // already been asked to add us to. On (re)connect we replay both sets
  // because SignalR drops group memberships on reconnect.
  private readonly joinedConversations = new Set<number>();
  private readonly pendingJoins = new Set<number>();
  private pendingAccountId: number | null = null;

  private readonly messageCreatedSubject = new Subject<Message>();
  private readonly conversationCreatedSubject = new Subject<Record<string, unknown>>();
  private readonly conversationStatusChangedSubject = new Subject<{ conversationId: number; accountId: number; previousStatus: string; newStatus: string }>();
  private readonly typingSubject = new Subject<TypingEvent>();
  private readonly presenceSubject = new Subject<PresenceEvent>();
  private readonly connectionStateSubject = new Subject<signalR.HubConnectionState>();

  readonly messageCreated$: Observable<Message> = this.messageCreatedSubject.asObservable();
  readonly conversationCreated$: Observable<Record<string, unknown>> = this.conversationCreatedSubject.asObservable();
  readonly conversationStatusChanged$: Observable<{ conversationId: number; accountId: number; previousStatus: string; newStatus: string }> = this.conversationStatusChangedSubject.asObservable();
  readonly typing$: Observable<TypingEvent> = this.typingSubject.asObservable();
  readonly presence$: Observable<PresenceEvent> = this.presenceSubject.asObservable();
  readonly connectionState$: Observable<signalR.HubConnectionState> = this.connectionStateSubject.asObservable();

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = this.authService.getToken();
    if (!token) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalrUrl, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.registerHandlers();

    this.connection.onreconnecting(() => {
      this.connectionStateSubject.next(signalR.HubConnectionState.Reconnecting);
    });

    this.connection.onreconnected(() => {
      this.connectionStateSubject.next(signalR.HubConnectionState.Connected);
      // SignalR loses all group memberships across a reconnect, so re-join
      // the account group and every conversation we had joined before.
      if (this.pendingAccountId !== null) {
        this.connection?.invoke('JoinAccountGroup', this.pendingAccountId).catch(() => { /* ignore */ });
      }
      for (const id of this.joinedConversations) {
        this.connection?.invoke('JoinConversation', id).catch(() => {
          this.pendingJoins.add(id);
        });
      }
    });

    this.connection.onclose(() => {
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    });

    try {
      await this.connection.start();
      this.connectionStateSubject.next(signalR.HubConnectionState.Connected);
      // Flush any joins requested before the connection finished coming up.
      for (const id of this.pendingJoins) {
        this.connection?.invoke('JoinConversation', id).catch(() => { /* will retry on reconnect */ });
      }
      this.pendingJoins.clear();
    } catch (error) {
      console.error('SignalR connection error:', error);
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    }
  }

  async joinAccountGroup(accountId: number): Promise<void> {
    this.pendingAccountId = accountId;
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinAccountGroup', accountId);
    }
  }

  async leaveAccountGroup(accountId: number): Promise<void> {
    if (this.pendingAccountId === accountId) {
      this.pendingAccountId = null;
    }
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveAccountGroup', accountId);
    }
  }

  async joinConversation(conversationId: number): Promise<void> {
    this.joinedConversations.add(conversationId);
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('JoinConversation', conversationId);
      } catch {
        this.pendingJoins.add(conversationId);
      }
    } else {
      // Queue the join so it fires once the connection is established.
      this.pendingJoins.add(conversationId);
    }
  }

  async leaveConversation(conversationId: number): Promise<void> {
    this.joinedConversations.delete(conversationId);
    this.pendingJoins.delete(conversationId);
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveConversation', conversationId);
    }
  }

  async sendTypingStatus(conversationId: number, isTyping: boolean): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('SendTypingStatus', conversationId, isTyping);
    }
  }

  async updatePresence(status: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('UpdatePresence', status);
    }
  }

  async disconnect(): Promise<void> {
    this.joinedConversations.clear();
    this.pendingJoins.clear();
    this.pendingAccountId = null;
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
    this.messageCreatedSubject.complete();
    this.conversationCreatedSubject.complete();
    this.conversationStatusChangedSubject.complete();
    this.typingSubject.complete();
    this.presenceSubject.complete();
    this.connectionStateSubject.complete();
  }

  private registerHandlers(): void {
    if (!this.connection) return;

    this.connection.on('message.created', (message: Message) => {
      this.messageCreatedSubject.next(message);
    });

    this.connection.on('conversation.created', (data: Record<string, unknown>) => {
      this.conversationCreatedSubject.next(data);
    });

    this.connection.on('conversation.status_changed', (data: { conversationId: number; accountId: number; previousStatus: string; newStatus: string }) => {
      this.conversationStatusChangedSubject.next(data);
    });

    this.connection.on('TypingStatus', (event: TypingEvent) => {
      this.typingSubject.next(event);
    });

    this.connection.on('presence', (event: PresenceEvent) => {
      this.presenceSubject.next(event);
    });
  }
}
