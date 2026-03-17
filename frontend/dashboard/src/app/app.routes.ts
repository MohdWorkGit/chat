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
    ],
  },
  {
    path: '',
    redirectTo: '/conversations',
    pathMatch: 'full',
  },
];
