import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ContactsState, contactsAdapter } from './contacts.reducer';

export const selectContactsState = createFeatureSelector<ContactsState>('contacts');

const { selectAll, selectEntities, selectTotal } = contactsAdapter.getSelectors();

export const selectAllContacts = createSelector(selectContactsState, selectAll);

export const selectContactEntities = createSelector(selectContactsState, selectEntities);

export const selectContactsTotal = createSelector(selectContactsState, selectTotal);

export const selectSelectedContactId = createSelector(selectContactsState, (state) => state.selectedContactId);

export const selectSelectedContact = createSelector(
  selectContactEntities,
  selectSelectedContactId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);

export const selectContactsLoading = createSelector(selectContactsState, (state) => state.loading);

export const selectContactsError = createSelector(selectContactsState, (state) => state.error);

export const selectContactsPagination = createSelector(selectContactsState, (state) => ({
  currentPage: state.currentPage,
  totalPages: state.totalPages,
  totalCount: state.totalCount,
}));
