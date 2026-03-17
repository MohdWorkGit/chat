import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ConversationsState, conversationsAdapter } from './conversations.reducer';

export const selectConversationsState = createFeatureSelector<ConversationsState>('conversations');

const { selectAll, selectEntities, selectTotal } = conversationsAdapter.getSelectors();

export const selectAllConversations = createSelector(selectConversationsState, selectAll);

export const selectConversationEntities = createSelector(selectConversationsState, selectEntities);

export const selectConversationsTotal = createSelector(selectConversationsState, selectTotal);

export const selectSelectedConversationId = createSelector(
  selectConversationsState,
  (state) => state.selectedConversationId
);

export const selectSelectedConversation = createSelector(
  selectConversationEntities,
  selectSelectedConversationId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);

export const selectConversationsLoading = createSelector(selectConversationsState, (state) => state.loading);

export const selectMessagesLoading = createSelector(selectConversationsState, (state) => state.messagesLoading);

export const selectConversationsError = createSelector(selectConversationsState, (state) => state.error);

export const selectConversationMessages = (conversationId: number) =>
  createSelector(selectConversationsState, (state) => state.messages[conversationId] || []);

export const selectConversationFilters = createSelector(selectConversationsState, (state) => state.filters);

export const selectConversationsPagination = createSelector(selectConversationsState, (state) => ({
  currentPage: state.currentPage,
  totalPages: state.totalPages,
  totalCount: state.totalCount,
}));
