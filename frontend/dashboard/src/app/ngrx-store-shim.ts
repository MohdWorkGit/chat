/**
 * Shim to resolve @ngrx/effects referencing @ngrx/store/src/models.TypedAction
 * which is not publicly exported in @ngrx/store v17.
 */
export interface TypedAction<T extends string> {
  readonly type: T;
}
