import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Article, Portal, Category } from '@core/models/helpcenter.model';
import { ApiError } from '@core/models/common.model';

export const HelpCenterActions = createActionGroup({
  source: 'HelpCenter',
  events: {
    'Load Articles': emptyProps(),
    'Load Articles Success': props<{ articles: Article[] }>(),
    'Load Articles Failure': props<{ error: ApiError }>(),

    'Load Article': props<{ id: number }>(),
    'Load Article Success': props<{ article: Article }>(),
    'Load Article Failure': props<{ error: ApiError }>(),

    'Create Article': props<{ data: Partial<Article> }>(),
    'Create Article Success': props<{ article: Article }>(),
    'Create Article Failure': props<{ error: ApiError }>(),

    'Update Article': props<{ id: number; data: Partial<Article> }>(),
    'Update Article Success': props<{ article: Article }>(),
    'Update Article Failure': props<{ error: ApiError }>(),

    'Delete Article': props<{ id: number }>(),
    'Delete Article Success': props<{ id: number }>(),
    'Delete Article Failure': props<{ error: ApiError }>(),

    'Load Portals': emptyProps(),
    'Load Portals Success': props<{ portals: Portal[] }>(),
    'Load Portals Failure': props<{ error: ApiError }>(),

    'Load Categories': props<{ portalId: number }>(),
    'Load Categories Success': props<{ categories: Category[] }>(),
    'Load Categories Failure': props<{ error: ApiError }>(),

    'Select Article': props<{ id: number | null }>(),

    'Clear Error': emptyProps(),
  },
});
