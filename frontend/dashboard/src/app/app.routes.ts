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
    path: 'auth/confirm-email',
    loadComponent: () =>
      import('@features/auth/confirm-email/confirm-email.component').then(
        (m) => m.ConfirmEmailComponent
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
        path: 'new',
        loadComponent: () =>
          import(
            '@features/conversations/conversation-create/conversation-create.component'
          ).then((m) => m.ConversationCreateComponent),
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
        path: 'new',
        loadComponent: () =>
          import(
            '@features/contacts/contact-create/contact-create.component'
          ).then((m) => m.ContactCreateComponent),
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
      {
        path: 'csat',
        loadComponent: () =>
          import(
            '@features/reports/csat/csat-report.component'
          ).then((m) => m.CsatReportComponent),
      },
      {
        path: 'agents',
        loadComponent: () =>
          import(
            '@features/reports/agents/agent-report.component'
          ).then((m) => m.AgentReportComponent),
      },
      {
        path: 'inboxes',
        loadComponent: () =>
          import(
            '@features/reports/inboxes/inbox-report.component'
          ).then((m) => m.InboxReportComponent),
      },
      {
        path: 'teams',
        loadComponent: () =>
          import(
            '@features/reports/teams/team-report.component'
          ).then((m) => m.TeamReportComponent),
      },
      {
        path: 'labels',
        loadComponent: () =>
          import(
            '@features/reports/labels/label-report.component'
          ).then((m) => m.LabelReportComponent),
      },
      {
        path: 'traffic',
        loadComponent: () =>
          import(
            '@features/reports/traffic/traffic-report.component'
          ).then((m) => m.TrafficReportComponent),
      },
      {
        path: 'bot-metrics',
        loadComponent: () =>
          import(
            '@features/reports/bot-metrics/bot-metrics-report.component'
          ).then((m) => m.BotMetricsReportComponent),
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
        children: [
          {
            path: 'inboxes',
            loadComponent: () =>
              import(
                '@features/inboxes/inbox-list/inbox-list.component'
              ).then((m) => m.InboxListComponent),
          },
          {
            path: 'inboxes/new',
            loadComponent: () =>
              import(
                '@features/inboxes/inbox-create/inbox-create.component'
              ).then((m) => m.InboxCreateComponent),
          },
          {
            path: 'inboxes/:id',
            loadComponent: () =>
              import(
                '@features/inboxes/inbox-detail/inbox-detail.component'
              ).then((m) => m.InboxDetailComponent),
          },
          {
            path: 'inboxes/:id/working-hours',
            loadComponent: () =>
              import(
                '@features/settings/working-hours/working-hours.component'
              ).then((m) => m.WorkingHoursComponent),
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
          {
            path: 'automations',
            loadComponent: () =>
              import(
                '@features/automations/automation-list/automation-list.component'
              ).then((m) => m.AutomationListComponent),
          },
          {
            path: 'automations/new',
            loadComponent: () =>
              import(
                '@features/automations/automation-form/automation-form.component'
              ).then((m) => m.AutomationFormComponent),
          },
          {
            path: 'campaigns',
            loadComponent: () =>
              import(
                '@features/campaigns/campaign-list/campaign-list.component'
              ).then((m) => m.CampaignListComponent),
          },
          {
            path: 'canned-responses',
            loadComponent: () =>
              import(
                '@features/canned-responses/canned-response-list/canned-response-list.component'
              ).then((m) => m.CannedResponseListComponent),
          },
          {
            path: 'canned-responses/new',
            loadComponent: () =>
              import(
                '@features/canned-responses/canned-response-form/canned-response-form.component'
              ).then((m) => m.CannedResponseFormComponent),
          },
          {
            path: 'macros',
            loadComponent: () =>
              import(
                '@features/macros/macro-list/macro-list.component'
              ).then((m) => m.MacroListComponent),
          },
          {
            path: 'macros/new',
            loadComponent: () =>
              import(
                '@features/macros/macro-form/macro-form.component'
              ).then((m) => m.MacroFormComponent),
          },
          {
            path: 'macros/:id',
            loadComponent: () =>
              import(
                '@features/macros/macro-form/macro-form.component'
              ).then((m) => m.MacroFormComponent),
          },
          {
            path: 'custom-attributes',
            loadComponent: () =>
              import(
                '@features/settings/custom-attributes/custom-attributes-list.component'
              ).then((m) => m.CustomAttributesListComponent),
          },
          {
            path: 'custom-attributes/new',
            loadComponent: () =>
              import(
                '@features/settings/custom-attributes/custom-attribute-form.component'
              ).then((m) => m.CustomAttributeFormComponent),
          },
          {
            path: 'custom-attributes/:id',
            loadComponent: () =>
              import(
                '@features/settings/custom-attributes/custom-attribute-form.component'
              ).then((m) => m.CustomAttributeFormComponent),
          },
          {
            path: 'profile',
            loadComponent: () =>
              import(
                '@features/settings/profile-settings/profile-settings.component'
              ).then((m) => m.ProfileSettingsComponent),
          },
          {
            path: 'account',
            loadComponent: () =>
              import(
                '@features/settings/account-settings/account-settings.component'
              ).then((m) => m.AccountSettingsComponent),
          },
          {
            path: 'notifications',
            loadComponent: () =>
              import(
                '@features/settings/notification-preferences/notification-preferences.component'
              ).then((m) => m.NotificationPreferencesComponent),
          },
          {
            path: 'agents',
            loadComponent: () =>
              import(
                '@features/settings/agents/agent-list/agent-list.component'
              ).then((m) => m.AgentListComponent),
          },
          {
            path: 'webhooks',
            loadComponent: () =>
              import(
                '@features/webhooks/webhook-list/webhook-list.component'
              ).then((m) => m.WebhookListComponent),
          },
          {
            path: 'custom-filters',
            loadComponent: () =>
              import(
                '@features/custom-filters/filter-list/filter-list.component'
              ).then((m) => m.FilterListComponent),
          },
          {
            path: 'custom-roles',
            loadComponent: () =>
              import(
                '@features/settings/custom-roles/custom-roles.component'
              ).then((m) => m.CustomRolesComponent),
          },
          {
            path: 'saml',
            loadComponent: () =>
              import(
                '@features/settings/saml-config/saml-config.component'
              ).then((m) => m.SamlConfigComponent),
          },
          {
            path: 'audit-logs',
            loadComponent: () =>
              import(
                '@features/settings/audit-logs/audit-logs.component'
              ).then((m) => m.AuditLogsComponent),
          },
          {
            path: 'email-templates',
            loadComponent: () =>
              import(
                '@features/settings/email-templates/email-template-list.component'
              ).then((m) => m.EmailTemplateListComponent),
          },
          {
            path: 'email-templates/new',
            loadComponent: () =>
              import(
                '@features/settings/email-templates/email-template-form.component'
              ).then((m) => m.EmailTemplateFormComponent),
          },
          {
            path: 'email-templates/:id',
            loadComponent: () =>
              import(
                '@features/settings/email-templates/email-template-form.component'
              ).then((m) => m.EmailTemplateFormComponent),
          },
        ],
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
      {
        path: 'new',
        loadComponent: () =>
          import(
            '@features/helpcenter/article-form/article-form.component'
          ).then((m) => m.ArticleFormComponent),
      },
      {
        path: ':id',
        loadComponent: () =>
          import(
            '@features/helpcenter/article-form/article-form.component'
          ).then((m) => m.ArticleFormComponent),
      },
    ],
  },
  {
    path: 'captain',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/captain/assistant-list/assistant-list.component'
          ).then((m) => m.AssistantListComponent),
      },
      {
        path: ':id/documents',
        loadComponent: () =>
          import(
            '@features/captain/document-manager/document-manager.component'
          ).then((m) => m.DocumentManagerComponent),
      },
      {
        path: ':id/scenarios',
        loadComponent: () =>
          import(
            '@features/captain/scenarios/captain-scenarios.component'
          ).then((m) => m.CaptainScenariosComponent),
      },
      {
        path: ':id/tools',
        loadComponent: () =>
          import(
            '@features/captain/custom-tools/captain-custom-tools.component'
          ).then((m) => m.CaptainCustomToolsComponent),
      },
    ],
  },
  {
    path: 'super-admin',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/super-admin/admin-dashboard/admin-dashboard.component'
          ).then((m) => m.AdminDashboardComponent),
      },
      {
        path: 'users',
        loadComponent: () =>
          import(
            '@features/super-admin/admin-users/admin-users.component'
          ).then((m) => m.AdminUsersComponent),
      },
    ],
  },
  {
    path: 'dashboard',
    redirectTo: '',
    pathMatch: 'full',
  },
  {
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            '@features/dashboard-home/dashboard-home.component'
          ).then((m) => m.DashboardHomeComponent),
      },
    ],
  },
];
