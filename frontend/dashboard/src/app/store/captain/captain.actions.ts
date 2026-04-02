import { createActionGroup, props } from '@ngrx/store';
import { CaptainAssistant, CaptainDocument } from '@core/services/captain.service';
import { ApiError } from '@core/models/common.model';

export interface CopilotMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: string;
}

export const CaptainActions = createActionGroup({
  source: 'Captain',
  events: {
    'Load Assistants': props<{ accountId: number }>(),
    'Load Assistants Success': props<{ assistants: CaptainAssistant[] }>(),
    'Load Assistants Failure': props<{ error: ApiError }>(),

    'Create Assistant': props<{ accountId: number; data: Partial<CaptainAssistant> }>(),
    'Create Assistant Success': props<{ assistant: CaptainAssistant }>(),
    'Create Assistant Failure': props<{ error: ApiError }>(),

    'Update Assistant': props<{ accountId: number; id: number; data: Partial<CaptainAssistant> }>(),
    'Update Assistant Success': props<{ assistant: CaptainAssistant }>(),
    'Update Assistant Failure': props<{ error: ApiError }>(),

    'Delete Assistant': props<{ accountId: number; id: number }>(),
    'Delete Assistant Success': props<{ id: number }>(),
    'Delete Assistant Failure': props<{ error: ApiError }>(),

    'Load Documents': props<{ accountId: number; assistantId: number }>(),
    'Load Documents Success': props<{ documents: CaptainDocument[] }>(),
    'Load Documents Failure': props<{ error: ApiError }>(),

    'Upload Document': props<{ accountId: number; assistantId: number; file: File }>(),
    'Upload Document Success': props<{ document: CaptainDocument }>(),
    'Upload Document Failure': props<{ error: ApiError }>(),

    'Delete Document': props<{ accountId: number; assistantId: number; docId: number }>(),
    'Delete Document Success': props<{ docId: number }>(),
    'Delete Document Failure': props<{ error: ApiError }>(),

    'Send Copilot Message': props<{ conversationId: number; message: string }>(),
    'Copilot Response Received': props<{ response: CopilotMessage }>(),
  },
});
