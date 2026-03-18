import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'auth/login',
    loadComponent: () =>
      import('@features/auth/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },
  {
    path: 'auth/register',
    loadComponent: () =>
      import('@features/auth/register/register.component').then(
        (m) => m.RegisterComponent
      ),
  },
  {
    path: 'auth/forgot-password',
    loadComponent: () =>
      import('@features/auth/forgot-password/forgot-password.component').then(
        (m) => m.ForgotPasswordComponent
      ),
  },
  {
    path: 'conversations',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/conversations/conversation-list/conversation-list.component'
          ).then((m) => m.ConversationListComponent),
      },
      {
        path: ':id',
        loadComponent: () =>
          import(
            '@features/conversations/conversation-detail/conversation-detail.component'
          ).then((m) => m.ConversationDetailComponent),
      },
    ],
  },
  {
    path: 'contacts',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/contacts/contact-list/contact-list.component'
          ).then((m) => m.ContactListComponent),
      },
      {
        path: ':id',
        loadComponent: () =>
          import(
            '@features/contacts/contact-detail/contact-detail.component'
          ).then((m) => m.ContactDetailComponent),
      },
    ],
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/reports/overview/overview.component'
          ).then((m) => m.ReportsOverviewComponent),
      },
    ],
  },
  {
    path: 'settings',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/settings/settings-layout/settings-layout.component'
          ).then((m) => m.SettingsLayoutComponent),
      },
      {
        path: 'inboxes',
        loadComponent: () =>
          import(
            '@features/inboxes/inbox-list/inbox-list.component'
          ).then((m) => m.InboxListComponent),
      },
      {
        path: 'inboxes/:id',
        loadComponent: () =>
          import(
            '@features/inboxes/inbox-detail/inbox-detail.component'
          ).then((m) => m.InboxDetailComponent),
      },
      {
        path: 'teams',
        loadComponent: () =>
          import(
            '@features/teams/team-list/team-list.component'
          ).then((m) => m.TeamListComponent),
      },
      {
        path: 'teams/:id',
        loadComponent: () =>
          import(
            '@features/teams/team-detail/team-detail.component'
          ).then((m) => m.TeamDetailComponent),
      },
      {
        path: 'labels',
        loadComponent: () =>
          import(
            '@features/labels/label-list/label-list.component'
          ).then((m) => m.LabelListComponent),
      },
    ],
  },
  {
    path: 'notifications',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/notifications/notification-center/notification-center.component'
          ).then((m) => m.NotificationCenterComponent),
      },
    ],
  },
  {
    path: 'helpcenter',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/helpcenter/article-list/article-list.component'
          ).then((m) => m.ArticleListComponent),
      },
    ],
  },
  {
    path: '',
    redirectTo: '/conversations',
    pathMatch: 'full',
  },
];
