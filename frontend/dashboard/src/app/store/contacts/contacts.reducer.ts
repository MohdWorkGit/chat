import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Contact } from '@core/models/contact.model';
import { ApiError } from '@core/models/common.model';
import { ContactsActions } from './contacts.actions';

export interface ContactsState extends EntityState<Contact> {
  selectedContactId: number | null;
  totalCount: number;
  currentPage: number;
  totalPages: number;
  loading: boolean;
  error: ApiError | null;
}

export const contactsAdapter: EntityAdapter<Contact> = createEntityAdapter<Contact>({
  selectId: (contact) => contact.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialContactsState: ContactsState = contactsAdapter.getInitialState({
  selectedContactId: null,
  totalCount: 0,
  currentPage: 1,
  totalPages: 0,
  loading: false,
  error: null,
});

export const contactsReducer = createReducer(
  initialContactsState,

  on(ContactsActions.loadContacts, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(ContactsActions.loadContactsSuccess, (state, { result }) =>
    contactsAdapter.setAll(result.data, {
      ...state,
      totalCount: result.meta.totalCount,
      currentPage: result.meta.currentPage,
      totalPages: result.meta.totalPages,
      loading: false,
    })
  ),

  on(ContactsActions.loadContactsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(ContactsActions.loadContactSuccess, (state, { contact }) =>
    contactsAdapter.upsertOne(contact, state)
  ),

  on(ContactsActions.createContactSuccess, (state, { contact }) =>
    contactsAdapter.addOne(contact, { ...state, totalCount: state.totalCount + 1 })
  ),

  on(ContactsActions.updateContactSuccess, (state, { contact }) =>
    contactsAdapter.updateOne({ id: contact.id, changes: contact }, state)
  ),

  on(ContactsActions.deleteContactSuccess, (state, { id }) =>
    contactsAdapter.removeOne(id, { ...state, totalCount: state.totalCount - 1 })
  ),

  on(ContactsActions.selectContact, (state, { id }) => ({
    ...state,
    selectedContactId: id,
  })),

  on(ContactsActions.searchContactsSuccess, (state, { result }) =>
    contactsAdapter.setAll(result.data, {
      ...state,
      totalCount: result.meta.totalCount,
      currentPage: result.meta.currentPage,
      totalPages: result.meta.totalPages,
      loading: false,
    })
  ),

  on(ContactsActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
