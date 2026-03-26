import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { CustomAttribute } from '@core/models/custom-attribute.model';
import { ApiError } from '@core/models/common.model';
import { CustomAttributesActions } from './custom-attributes.actions';

export interface CustomAttributesState extends EntityState<CustomAttribute> {
  loading: boolean;
  error: ApiError | null;
}

export const customAttributesAdapter: EntityAdapter<CustomAttribute> = createEntityAdapter<CustomAttribute>({
  selectId: (attr) => attr.id,
  sortComparer: (a, b) => a.displayName.localeCompare(b.displayName),
});

export const initialCustomAttributesState: CustomAttributesState = customAttributesAdapter.getInitialState({
  loading: false,
  error: null,
});

export const customAttributesReducer = createReducer(
  initialCustomAttributesState,

  on(CustomAttributesActions.loadCustomAttributes, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CustomAttributesActions.loadCustomAttributesSuccess, (state, { customAttributes }) =>
    customAttributesAdapter.setAll(customAttributes, { ...state, loading: false })
  ),

  on(CustomAttributesActions.loadCustomAttributesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(CustomAttributesActions.createCustomAttributeSuccess, (state, { customAttribute }) =>
    customAttributesAdapter.addOne(customAttribute, state)
  ),

  on(CustomAttributesActions.updateCustomAttributeSuccess, (state, { customAttribute }) =>
    customAttributesAdapter.updateOne({ id: customAttribute.id, changes: customAttribute }, state)
  ),

  on(CustomAttributesActions.deleteCustomAttributeSuccess, (state, { id }) =>
    customAttributesAdapter.removeOne(id, state)
  ),

  on(CustomAttributesActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
