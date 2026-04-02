import { createReducer, on } from '@ngrx/store';
import { CaptainAssistant, CaptainDocument } from '@core/services/captain.service';
import { ApiError } from '@core/models/common.model';
import { CaptainActions, CopilotMessage } from './captain.actions';

export interface CaptainState {
  assistants: CaptainAssistant[];
  documents: CaptainDocument[];
  copilotMessages: CopilotMessage[];
  loading: boolean;
  error: ApiError | null;
}

export const initialCaptainState: CaptainState = {
  assistants: [],
  documents: [],
  copilotMessages: [],
  loading: false,
  error: null,
};

export const captainReducer = createReducer(
  initialCaptainState,

  // Load Assistants
  on(CaptainActions.loadAssistants, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CaptainActions.loadAssistantsSuccess, (state, { assistants }) => ({
    ...state,
    assistants,
    loading: false,
  })),

  on(CaptainActions.loadAssistantsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Create Assistant
  on(CaptainActions.createAssistant, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CaptainActions.createAssistantSuccess, (state, { assistant }) => ({
    ...state,
    assistants: [...state.assistants, assistant],
    loading: false,
  })),

  on(CaptainActions.createAssistantFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Update Assistant
  on(CaptainActions.updateAssistant, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CaptainActions.updateAssistantSuccess, (state, { assistant }) => ({
    ...state,
    assistants: state.assistants.map((a) => (a.id === assistant.id ? assistant : a)),
    loading: false,
  })),

  on(CaptainActions.updateAssistantFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Delete Assistant
  on(CaptainActions.deleteAssistant, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CaptainActions.deleteAssistantSuccess, (state, { id }) => ({
    ...state,
    assistants: state.assistants.filter((a) => a.id !== id),
    loading: false,
  })),

  on(CaptainActions.deleteAssistantFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Load Documents
  on(CaptainActions.loadDocuments, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CaptainActions.loadDocumentsSuccess, (state, { documents }) => ({
    ...state,
    documents,
    loading: false,
  })),

  on(CaptainActions.loadDocumentsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Upload Document
  on(CaptainActions.uploadDocument, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CaptainActions.uploadDocumentSuccess, (state, { document }) => ({
    ...state,
    documents: [...state.documents, document],
    loading: false,
  })),

  on(CaptainActions.uploadDocumentFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Delete Document
  on(CaptainActions.deleteDocument, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CaptainActions.deleteDocumentSuccess, (state, { docId }) => ({
    ...state,
    documents: state.documents.filter((d) => d.id !== docId),
    loading: false,
  })),

  on(CaptainActions.deleteDocumentFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Copilot Messages
  on(CaptainActions.sendCopilotMessage, (state, { message }) => ({
    ...state,
    copilotMessages: [
      ...state.copilotMessages,
      {
        id: crypto.randomUUID(),
        role: 'user' as const,
        content: message,
        timestamp: new Date().toISOString(),
      },
    ],
  })),

  on(CaptainActions.copilotResponseReceived, (state, { response }) => ({
    ...state,
    copilotMessages: [...state.copilotMessages, response],
  }))
);
