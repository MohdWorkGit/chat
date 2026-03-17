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

  private readonly messageCreatedSubject = new Subject<Message>();
  private readonly conversationUpdatedSubject = new Subject<{ conversationId: number; updates: Record<string, unknown> }>();
  private readonly typingSubject = new Subject<TypingEvent>();
  private readonly presenceSubject = new Subject<PresenceEvent>();
  private readonly connectionStateSubject = new Subject<signalR.HubConnectionState>();

  readonly messageCreated$: Observable<Message> = this.messageCreatedSubject.asObservable();
  readonly conversationUpdated$: Observable<{ conversationId: number; updates: Record<string, unknown> }> = this.conversationUpdatedSubject.asObservable();
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
    });

    this.connection.onclose(() => {
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    });

    try {
      await this.connection.start();
      this.connectionStateSubject.next(signalR.HubConnectionState.Connected);
    } catch (error) {
      console.error('SignalR connection error:', error);
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    }
  }

  async joinAccountGroup(accountId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinAccountGroup', accountId);
    }
  }

  async leaveAccountGroup(accountId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveAccountGroup', accountId);
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
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
    this.messageCreatedSubject.complete();
    this.conversationUpdatedSubject.complete();
    this.typingSubject.complete();
    this.presenceSubject.complete();
    this.connectionStateSubject.complete();
  }

  private registerHandlers(): void {
    if (!this.connection) return;

    this.connection.on('message.created', (message: Message) => {
      this.messageCreatedSubject.next(message);
    });

    this.connection.on('conversation.updated', (data: { conversationId: number; updates: Record<string, unknown> }) => {
      this.conversationUpdatedSubject.next(data);
    });

    this.connection.on('typing', (event: TypingEvent) => {
      this.typingSubject.next(event);
    });

    this.connection.on('presence', (event: PresenceEvent) => {
      this.presenceSubject.next(event);
    });
  }
}
