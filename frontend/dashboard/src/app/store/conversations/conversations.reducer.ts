import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Conversation, Message } from '@core/models/conversation.model';
import { ApiError } from '@core/models/common.model';
import { ConversationFilters } from '@core/services/conversation.service';
import { ConversationsActions } from './conversations.actions';

export interface ConversationsState extends EntityState<Conversation> {
  selectedConversationId: number | null;
  messages: Record<number, Message[]>;
  filters: ConversationFilters;
  totalCount: number;
  currentPage: number;
  totalPages: number;
  loading: boolean;
  messagesLoading: boolean;
  error: ApiError | null;
}

export const conversationsAdapter: EntityAdapter<Conversation> = createEntityAdapter<Conversation>({
  selectId: (conversation) => conversation.id,
  sortComparer: (a, b) => new Date(b.lastActivityAt).getTime() - new Date(a.lastActivityAt).getTime(),
});

export const initialConversationsState: ConversationsState = conversationsAdapter.getInitialState({
  selectedConversationId: null,
  messages: {},
  filters: {},
  totalCount: 0,
  currentPage: 1,
  totalPages: 0,
  loading: false,
  messagesLoading: false,
  error: null,
});

export const conversationsReducer = createReducer(
  initialConversationsState,

  on(ConversationsActions.loadConversations, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(ConversationsActions.loadConversationsSuccess, (state, { result }) =>
    conversationsAdapter.setAll(result.data, {
      ...state,
      totalCount: result.meta.totalCount,
      currentPage: result.meta.currentPage,
      totalPages: result.meta.totalPages,
      loading: false,
    })
  ),

  on(ConversationsActions.loadConversationsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(ConversationsActions.loadConversationSuccess, (state, { conversation }) =>
    conversationsAdapter.upsertOne(conversation, state)
  ),

  on(ConversationsActions.selectConversation, (state, { id }) => ({
    ...state,
    selectedConversationId: id,
  })),

  on(ConversationsActions.updateConversationStatusSuccess, (state, { conversation }) =>
    conversationsAdapter.updateOne({ id: conversation.id, changes: conversation }, state)
  ),

  on(ConversationsActions.assignConversationSuccess, (state, { conversation }) =>
    conversationsAdapter.updateOne({ id: conversation.id, changes: conversation }, state)
  ),

  on(ConversationsActions.loadMessages, (state) => ({
    ...state,
    messagesLoading: true,
  })),

  on(ConversationsActions.loadMessagesSuccess, (state, { conversationId, result }) => ({
    ...state,
    messages: {
      ...state.messages,
      [conversationId]: result.data,
    },
    messagesLoading: false,
  })),

  on(ConversationsActions.loadMessagesFailure, (state) => ({
    ...state,
    messagesLoading: false,
  })),

  on(ConversationsActions.sendMessageSuccess, (state, { message }) => ({
    ...state,
    messages: {
      ...state.messages,
      [message.conversationId]: [...(state.messages[message.conversationId] || []), message],
    },
  })),

  on(ConversationsActions.messageReceived, (state, { message }) => ({
    ...state,
    messages: {
      ...state.messages,
      [message.conversationId]: [...(state.messages[message.conversationId] || []), message],
    },
  })),

  on(ConversationsActions.setFilters, (state, { filters }) => ({
    ...state,
    filters,
  })),

  on(ConversationsActions.clearFilters, (state) => ({
    ...state,
    filters: {},
  }))
);
