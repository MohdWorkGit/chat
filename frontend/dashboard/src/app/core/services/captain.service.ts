import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CaptainAssistant {
  id: number;
  accountId: number;
  name: string;
  description: string;
  model: string;
  temperature: number;
  responseGuidelines: string;
  guardrails: string;
  createdAt: string;
  updatedAt: string;
}

export interface CaptainDocument {
  id: number;
  assistantId: number;
  fileName: string;
  fileUrl: string;
  contentType: string;
  processedAt: string | null;
  createdAt: string;
}

export interface CopilotSuggestion {
  text: string;
  confidence: number;
}

export interface RewriteResult {
  original: string;
  rewritten: string;
  tone: string;
}

export interface ConversationSummary {
  summary: string;
  keyPoints: string[];
}

@Injectable({
  providedIn: 'root',
})
export class CaptainService {
  private readonly api = inject(ApiService);

  private basePath(accountId: number): string {
    return `/accounts/${accountId}/captain`;
  }

  getAssistants(accountId: number): Observable<CaptainAssistant[]> {
    return this.api.get<CaptainAssistant[]>(`${this.basePath(accountId)}/assistants`);
  }

  getAssistant(accountId: number, id: number): Observable<CaptainAssistant> {
    return this.api.get<CaptainAssistant>(`${this.basePath(accountId)}/assistants/${id}`);
  }

  createAssistant(accountId: number, data: Partial<CaptainAssistant>): Observable<CaptainAssistant> {
    return this.api.post<CaptainAssistant>(`${this.basePath(accountId)}/assistants`, data);
  }

  updateAssistant(accountId: number, id: number, data: Partial<CaptainAssistant>): Observable<CaptainAssistant> {
    return this.api.put<CaptainAssistant>(`${this.basePath(accountId)}/assistants/${id}`, data);
  }

  deleteAssistant(accountId: number, id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath(accountId)}/assistants/${id}`);
  }

  getDocuments(accountId: number, assistantId: number): Observable<CaptainDocument[]> {
    return this.api.get<CaptainDocument[]>(
      `${this.basePath(accountId)}/assistants/${assistantId}/documents`
    );
  }

  uploadDocument(accountId: number, assistantId: number, file: File): Observable<CaptainDocument> {
    const formData = new FormData();
    formData.append('file', file);
    return this.api.upload<CaptainDocument>(
      `${this.basePath(accountId)}/assistants/${assistantId}/documents`,
      formData
    );
  }

  deleteDocument(accountId: number, assistantId: number, docId: number): Observable<void> {
    return this.api.delete<void>(
      `${this.basePath(accountId)}/assistants/${assistantId}/documents/${docId}`
    );
  }

  // --- Copilot (agent-facing AI assistant) ---
  private copilotPath(accountId: number): string {
    return `/accounts/${accountId}/copilot`;
  }

  suggestReply(accountId: number, conversationId: number): Observable<CopilotSuggestion> {
    return this.api.post<CopilotSuggestion>(`${this.copilotPath(accountId)}/suggest`, {
      conversationId,
    });
  }

  rewriteText(
    accountId: number,
    text: string,
    tone: string = 'professional'
  ): Observable<RewriteResult> {
    return this.api.post<RewriteResult>(`${this.copilotPath(accountId)}/rewrite`, {
      text,
      tone,
    });
  }

  summarizeConversation(
    accountId: number,
    conversationId: number
  ): Observable<ConversationSummary> {
    return this.api.post<ConversationSummary>(`${this.copilotPath(accountId)}/summarize`, {
      conversationId,
    });
  }

  suggestLabels(accountId: number, conversationId: number): Observable<string[]> {
    return this.api.post<string[]>(`${this.copilotPath(accountId)}/suggest-labels`, {
      conversationId,
    });
  }

  suggestFollowUp(accountId: number, conversationId: number): Observable<string[]> {
    return this.api.post<string[]>(`${this.copilotPath(accountId)}/suggest-follow-up`, {
      conversationId,
    });
  }
}
