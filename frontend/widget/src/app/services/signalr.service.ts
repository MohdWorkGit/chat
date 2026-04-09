import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { Message } from './widget-api.service';

export interface CampaignMessage {
  message: string;
  senderName: string;
  avatarUrl: string;
}

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection: signalR.HubConnection | null = null;

  private readonly messagesSubject = new Subject<Message>();
  private readonly typingSubject = new Subject<boolean>();
  private readonly conversationResolvedSubject = new Subject<number>();
  private readonly campaignMessageSubject = new Subject<CampaignMessage>();

  readonly messages$ = this.messagesSubject.asObservable();
  readonly typing$ = this.typingSubject.asObservable();
  readonly conversationResolved$ = this.conversationResolvedSubject.asObservable();
  readonly campaignMessage$ = this.campaignMessageSubject.asObservable();

  initialize(_websiteToken: string): void {
    // The widget has no user JWT — the backend ConversationHub allows
    // anonymous connections and relies on per-conversation group joins.
    // We deliberately do NOT pass the website token here because the
    // JwtBearerHandler would otherwise reject it as an invalid token.
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/conversation', {
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

    this.hubConnection.on('message.created', (message: Message) => {
      this.messagesSubject.next(message);
    });

    this.hubConnection.on('TypingStatus', (data: { isTyping: boolean }) => {
      this.typingSubject.next(data.isTyping);
    });

    this.hubConnection.on('conversation.status_changed', (data: { conversationId: number; newStatus: string }) => {
      if (data.newStatus === 'Resolved') {
        this.conversationResolvedSubject.next(data.conversationId);
      }
    });

    this.hubConnection.on('campaign.message', (data: CampaignMessage) => {
      this.campaignMessageSubject.next(data);
    });
  }

  private startConnection(): void {
    this.hubConnection?.start().catch(err => {
      console.error('SignalR widget connection error:', err);
      setTimeout(() => this.startConnection(), 5000);
    });
  }
}
