import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { Message } from './widget-api.service';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection: signalR.HubConnection | null = null;

  private readonly messagesSubject = new Subject<Message>();
  private readonly typingSubject = new Subject<boolean>();
  private readonly conversationResolvedSubject = new Subject<number>();

  readonly messages$ = this.messagesSubject.asObservable();
  readonly typing$ = this.typingSubject.asObservable();
  readonly conversationResolved$ = this.conversationResolvedSubject.asObservable();

  initialize(websiteToken: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/widget', {
        accessTokenFactory: () => websiteToken,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.registerHandlers();
    this.startConnection();
  }

  joinConversation(conversationId: number): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinConversation', conversationId);
    }
  }

  leaveConversation(conversationId: number): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('LeaveConversation', conversationId);
    }
  }

  disconnect(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveMessage', (message: Message) => {
      this.messagesSubject.next(message);
    });

    this.hubConnection.on('AgentTyping', (isTyping: boolean) => {
      this.typingSubject.next(isTyping);
    });

    this.hubConnection.on('ConversationResolved', (conversationId: number) => {
      this.conversationResolvedSubject.next(conversationId);
    });

    this.hubConnection.on('ConversationAssigned', (_agentName: string) => {
      // Handle agent assignment update
    });
  }

  private startConnection(): void {
    this.hubConnection?.start().catch(err => {
      console.error('SignalR widget connection error:', err);
      setTimeout(() => this.startConnection(), 5000);
    });
  }
}
