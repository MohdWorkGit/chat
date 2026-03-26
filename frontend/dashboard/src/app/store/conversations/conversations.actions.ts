import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Conversation, Message } from '@core/models/conversation.model';
import { ApiError, PaginatedResult } from '@core/models/common.model';
import { ConversationFilters } from '@core/services/conversation.service';

export const ConversationsActions = createActionGroup({
  source: 'Conversations',
  events: {
    'Load Conversations': props<{ filters?: ConversationFilters }>(),
    'Load Conversations Success': props<{ result: PaginatedResult<Conversation> }>(),
    'Load Conversations Failure': props<{ error: ApiError }>(),

    'Load Conversation': props<{ id: number }>(),
    'Load Conversation Success': props<{ conversation: Conversation }>(),
    'Load Conversation Failure': props<{ error: ApiError }>(),

    'Select Conversation': props<{ id: number | null }>(),

    'Update Conversation Status': props<{ id: number; status: string }>(),
    'Update Conversation Status Success': props<{ conversation: Conversation }>(),
    'Update Conversation Status Failure': props<{ error: ApiError }>(),

    'Assign Conversation': props<{ id: number; assigneeId: number }>(),
    'Assign Conversation Success': props<{ conversation: Conversation }>(),
    'Assign Conversation Failure': props<{ error: ApiError }>(),

    'Load Messages': props<{ conversationId: number; page?: number }>(),
    'Load Messages Success': props<{ conversationId: number; result: PaginatedResult<Message> }>(),
    'Load Messages Failure': props<{ error: ApiError }>(),

    'Send Message': props<{ conversationId: number; content: string; isPrivate?: boolean }>(),
    'Send Message Success': props<{ message: Message }>(),
    'Send Message Failure': props<{ error: ApiError }>(),

    'Message Received': props<{ message: Message }>(),
    'Conversation Updated': props<{ conversationId: number; updates: Record<string, unknown> }>(),

    'Create Conversation': props<{ data: Partial<Conversation> }>(),
    'Create Conversation Success': props<{ conversation: Conversation }>(),
    'Create Conversation Failure': props<{ error: ApiError }>(),

    'Set Filters': props<{ filters: ConversationFilters }>(),
    'Clear Filters': emptyProps(),
  },
});
