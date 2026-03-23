import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Article, Portal, Category } from '@core/models/helpcenter.model';
import { ApiError } from '@core/models/common.model';
import { HelpCenterActions } from './helpcenter.actions';

export interface HelpCenterState extends EntityState<Article> {
  selectedArticleId: number | null;
  portals: Portal[];
  categories: Category[];
  loading: boolean;
  error: ApiError | null;
}

export const helpCenterAdapter: EntityAdapter<Article> = createEntityAdapter<Article>({
  selectId: (article) => article.id,
  sortComparer: (a, b) => a.title.localeCompare(b.title),
});

export const initialHelpCenterState: HelpCenterState = helpCenterAdapter.getInitialState({
  selectedArticleId: null,
  portals: [],
  categories: [],
  loading: false,
  error: null,
});

export const helpCenterReducer = createReducer(
  initialHelpCenterState,

  on(HelpCenterActions.loadArticles, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(HelpCenterActions.loadArticlesSuccess, (state, { articles }) =>
    helpCenterAdapter.setAll(articles, {
      ...state,
      loading: false,
    })
  ),

  on(HelpCenterActions.loadArticlesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(HelpCenterActions.loadArticleSuccess, (state, { article }) =>
    helpCenterAdapter.upsertOne(article, state)
  ),

  on(HelpCenterActions.createArticleSuccess, (state, { article }) =>
    helpCenterAdapter.addOne(article, state)
  ),

  on(HelpCenterActions.updateArticleSuccess, (state, { article }) =>
    helpCenterAdapter.updateOne({ id: article.id, changes: article }, state)
  ),

  on(HelpCenterActions.deleteArticleSuccess, (state, { id }) =>
    helpCenterAdapter.removeOne(id, state)
  ),

  on(HelpCenterActions.loadPortals, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(HelpCenterActions.loadPortalsSuccess, (state, { portals }) => ({
    ...state,
    portals,
    loading: false,
  })),

  on(HelpCenterActions.loadPortalsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(HelpCenterActions.loadCategories, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(HelpCenterActions.loadCategoriesSuccess, (state, { categories }) => ({
    ...state,
    categories,
    loading: false,
  })),

  on(HelpCenterActions.loadCategoriesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(HelpCenterActions.selectArticle, (state, { id }) => ({
    ...state,
    selectedArticleId: id,
  })),

  on(HelpCenterActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
