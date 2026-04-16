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
  private readonly joinedConversations = new Set<number>();
  private readonly pendingJoins = new Set<number>();

  private readonly messagesSubject = new Subject<Message>();
  private readonly typingSubject = new Subject<boolean>();
  private readonly conversationResolvedSubject = new Subject<number>();
  private readonly campaignMessageSubject = new Subject<CampaignMessage>();

  readonly messages$ = this.messagesSubject.asObservable();
  readonly typing$ = this.typingSubject.asObservable();
  readonly conversationResolved$ = this.conversationResolvedSubject.asObservable();
  readonly campaignMessage$ = this.campaignMessageSubject.asObservable();

  initialize(_websiteToken: string, apiOrigin = ''): void {
    // The widget has no user JWT — the backend ConversationHub allows
    // anonymous connections and relies on per-conversation group joins.
    // We deliberately do NOT pass the website token here because the
    // JwtBearerHandler would otherwise reject it as an invalid token.
    //
    // When the widget is embedded on a different origin than the API,
    // `apiOrigin` must be an absolute URL (e.g. https://api.example.com)
    // so the SignalR negotiate request reaches the backend and not the
    // CDN hosting the widget bundle.
    const hubUrl = `${(apiOrigin || '').replace(/\/+$/, '')}/hubs/conversation`;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.registerHandlers();
    // After an automatic reconnect SignalR loses all group memberships,
    // so re-join every conversation we had joined before.
    this.hubConnection.onreconnected(() => {
      for (const id of this.joinedConversations) {
        this.hubConnection?.invoke('JoinConversation', id).catch(() => {
          this.pendingJoins.add(id);
        });
      }
    });
    this.startConnection();
  }

  joinConversation(conversationId: number): void {
    this.joinedConversations.add(conversationId);
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinConversation', conversationId).catch(() => {
        this.pendingJoins.add(conversationId);
      });
    } else {
      // Queue the join so it fires once the connection is established.
      this.pendingJoins.add(conversationId);
    }
  }

  leaveConversation(conversationId: number): void {
    this.joinedConversations.delete(conversationId);
    this.pendingJoins.delete(conversationId);
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('LeaveConversation', conversationId);
    }
  }

  disconnect(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
    this.joinedConversations.clear();
    this.pendingJoins.clear();
  }

  /**
   * Normalizes a SignalR message payload to the camelCase shape the widget
   * expects.  ASP.NET Core SignalR may serialize with PascalCase unless
   * explicitly configured, while the REST API always returns camelCase.
   */
  private normalizeMessage(data: Record<string, unknown>): Message {
    return {
      id: (data['id'] ?? data['Id']) as number,
      conversationId: (data['conversationId'] ?? data['ConversationId']) as number,
      content: (data['content'] ?? data['Content']) as string,
      senderType: (data['senderType'] ?? data['SenderType']) as Message['senderType'],
      contentType: (data['contentType'] ?? data['ContentType']) as string,
      createdAt: (data['createdAt'] ?? data['CreatedAt']) as string,
    };
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('message.created', (data: Record<string, unknown>) => {
      this.messagesSubject.next(this.normalizeMessage(data));
    });

    this.hubConnection.on('TypingStatus', (data: { isTyping: boolean }) => {
      this.typingSubject.next(data.isTyping);
    });

    this.hubConnection.on('conversation.status_changed', (data: Record<string, unknown>) => {
      const newStatus = (data['newStatus'] ?? data['NewStatus']) as string;
      const conversationId = (data['conversationId'] ?? data['ConversationId']) as number;
      if (newStatus === 'Resolved') {
        this.conversationResolvedSubject.next(conversationId);
      }
    });

    this.hubConnection.on('campaign.message', (data: CampaignMessage) => {
      this.campaignMessageSubject.next(data);
    });
  }

  private startConnection(): void {
    this.hubConnection?.start().then(() => {
      // Flush any conversation joins requested before the connection
      // finished coming up.
      for (const id of this.pendingJoins) {
        this.hubConnection?.invoke('JoinConversation', id).catch(() => {
          /* will retry on reconnect */
        });
      }
      this.pendingJoins.clear();
    }).catch(err => {
      console.error('SignalR widget connection error:', err);
      setTimeout(() => this.startConnection(), 5000);
    });
  }
}
