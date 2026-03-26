import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EnterpriseService, SamlConfig, SamlRoleMapping } from '@core/services/enterprise.service';

const USER_ROLES = ['administrator', 'agent', 'supervisor'];

@Component({
  selector: 'app-saml-config',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6 max-w-4xl">
      <!-- Header -->
      <div class="mb-6">
        <h2 class="text-lg font-semibold text-gray-900">SAML Single Sign-On</h2>
        <p class="text-sm text-gray-500 mt-1">Configure SAML-based SSO for your organization.</p>
      </div>

      <!-- Loading State -->
      @if (loading) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        <!-- SAML Config Form -->
        <div class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-sm font-semibold text-gray-900">Identity Provider Configuration</h3>
            @if (samlConfig) {
              <label class="flex items-center gap-2 cursor-pointer">
                <span class="text-sm text-gray-600">{{ samlConfig.enabled ? 'Enabled' : 'Disabled' }}</span>
                <button
                  (click)="toggleEnabled()"
                  [class]="samlConfig.enabled
                    ? 'relative inline-flex h-6 w-11 items-center rounded-full bg-blue-600 transition-colors'
                    : 'relative inline-flex h-6 w-11 items-center rounded-full bg-gray-200 transition-colors'"
                >
                  <span
                    [class]="samlConfig.enabled
                      ? 'inline-block h-4 w-4 transform rounded-full bg-white transition-transform translate-x-6'
                      : 'inline-block h-4 w-4 transform rounded-full bg-white transition-transform translate-x-1'"
                  ></span>
                </button>
              </label>
            }
          </div>

          <form [formGroup]="samlForm" (ngSubmit)="saveSamlConfig()" class="space-y-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Entity ID (IdP)</label>
                <input
                  formControlName="idpEntityId"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="https://idp.example.com/entity"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">SSO URL</label>
                <input
                  formControlName="idpSsoTargetUrl"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="https://idp.example.com/sso"
                />
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">SP Entity ID</label>
                <input
                  formControlName="spEntityId"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="https://yourapp.example.com/saml/metadata"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Assertion Consumer Service URL</label>
                <input
                  formControlName="assertionConsumerServiceUrl"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="https://yourapp.example.com/saml/acs"
                />
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Name Identifier Format</label>
              <input
                formControlName="nameIdentifierFormat"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">IdP Certificate</label>
              <textarea
                formControlName="idpCertificate"
                rows="6"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm font-mono focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="-----BEGIN CERTIFICATE-----&#10;...&#10;-----END CERTIFICATE-----"
              ></textarea>
            </div>

            <div class="flex items-center justify-between pt-2">
              <div>
                @if (samlConfig) {
                  <button
                    type="button"
                    (click)="deleteSamlConfig()"
                    class="px-4 py-2 text-sm font-medium text-red-600 border border-red-300 rounded-lg hover:bg-red-50 transition-colors"
                  >
                    Delete Configuration
                  </button>
                }
              </div>
              <button
                type="submit"
                [disabled]="samlForm.invalid"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {{ samlConfig ? 'Update Configuration' : 'Save Configuration' }}
              </button>
            </div>
          </form>
        </div>

        <!-- Role Mappings -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <div class="flex items-center justify-between mb-4">
            <div>
              <h3 class="text-sm font-semibold text-gray-900">Role Mappings</h3>
              <p class="text-xs text-gray-500 mt-1">Map SAML attribute values to platform roles.</p>
            </div>
          </div>

          @if (roleMappings.length > 0) {
            <table class="min-w-full divide-y divide-gray-200 mb-4">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">SAML Attribute Value</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Platform Role</th>
                  <th class="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                @for (mapping of roleMappings; track mapping.id) {
                  <tr class="hover:bg-gray-50">
                    <td class="px-4 py-3 text-sm text-gray-900">{{ mapping.samlAttributeValue }}</td>
                    <td class="px-4 py-3">
                      <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800 capitalize">
                        {{ mapping.userRole }}
                      </span>
                    </td>
                    <td class="px-4 py-3 text-right">
                      <button
                        (click)="deleteRoleMapping(mapping)"
                        class="text-xs text-red-600 hover:text-red-800"
                      >
                        Remove
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          } @else {
            <p class="text-sm text-gray-500 mb-4">No role mappings configured.</p>
          }

          <!-- Add Mapping Form -->
          <form [formGroup]="mappingForm" (ngSubmit)="addRoleMapping()" class="flex items-end gap-3 pt-3 border-t border-gray-200">
            <div class="flex-1">
              <label class="block text-xs font-medium text-gray-700 mb-1">SAML Attribute Value</label>
              <input
                formControlName="samlAttributeValue"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="e.g., admin-group"
              />
            </div>
            <div class="w-48">
              <label class="block text-xs font-medium text-gray-700 mb-1">Platform Role</label>
              <select
                formControlName="userRole"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white"
              >
                <option value="" disabled>Select role</option>
                @for (role of userRoles; track role) {
                  <option [value]="role">{{ role | titlecase }}</option>
                }
              </select>
            </div>
            <button
              type="submit"
              [disabled]="mappingForm.invalid"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors whitespace-nowrap"
            >
              Add Mapping
            </button>
          </form>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class SamlConfigComponent implements OnInit {
  private enterpriseService = inject(EnterpriseService);
  private fb = inject(FormBuilder);

  samlConfig: SamlConfig | null = null;
  roleMappings: SamlRoleMapping[] = [];
  loading = true;
  userRoles = USER_ROLES;

  samlForm: FormGroup = this.fb.group({
    idpEntityId: ['', Validators.required],
    idpSsoTargetUrl: ['', Validators.required],
    idpCertificate: ['', Validators.required],
    spEntityId: ['', Validators.required],
    assertionConsumerServiceUrl: ['', Validators.required],
    nameIdentifierFormat: ['urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress'],
  });

  mappingForm: FormGroup = this.fb.group({
    samlAttributeValue: ['', Validators.required],
    userRole: ['', Validators.required],
  });

  ngOnInit(): void {
    this.loadSamlConfig();
  }

  loadSamlConfig(): void {
    this.loading = true;
    this.enterpriseService.getSamlConfig().subscribe({
      next: (config) => {
        this.samlConfig = config;
        this.samlForm.patchValue({
          idpEntityId: config.idpEntityId,
          idpSsoTargetUrl: config.idpSsoTargetUrl,
          idpCertificate: config.idpCertificate,
          spEntityId: config.spEntityId,
          assertionConsumerServiceUrl: config.assertionConsumerServiceUrl,
          nameIdentifierFormat: config.nameIdentifierFormat,
        });
        this.loadRoleMappings();
        this.loading = false;
      },
      error: () => {
        this.samlConfig = null;
        this.loading = false;
      },
    });
  }

  loadRoleMappings(): void {
    this.enterpriseService.getSamlRoleMappings().subscribe({
      next: (mappings) => {
        this.roleMappings = mappings;
      },
    });
  }

  saveSamlConfig(): void {
    if (this.samlForm.invalid) return;

    const data = this.samlForm.value;

    if (this.samlConfig) {
      this.enterpriseService.updateSamlConfig(data).subscribe({
        next: (config) => {
          this.samlConfig = config;
        },
      });
    } else {
      this.enterpriseService.createSamlConfig(data).subscribe({
        next: (config) => {
          this.samlConfig = config;
        },
      });
    }
  }

  toggleEnabled(): void {
    if (!this.samlConfig) return;

    this.enterpriseService
      .updateSamlConfig({ enabled: !this.samlConfig.enabled })
      .subscribe({
        next: (config) => {
          this.samlConfig = config;
        },
      });
  }

  deleteSamlConfig(): void {
    if (confirm('Are you sure you want to delete the SAML configuration? This will disable SSO for all users.')) {
      this.enterpriseService.deleteSamlConfig().subscribe({
        next: () => {
          this.samlConfig = null;
          this.roleMappings = [];
          this.samlForm.reset({
            nameIdentifierFormat: 'urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress',
          });
        },
      });
    }
  }

  addRoleMapping(): void {
    if (this.mappingForm.invalid) return;

    const data = this.mappingForm.value;
    this.enterpriseService.createSamlRoleMapping(data).subscribe({
      next: () => {
        this.mappingForm.reset({ samlAttributeValue: '', userRole: '' });
        this.loadRoleMappings();
      },
    });
  }

  deleteRoleMapping(mapping: SamlRoleMapping): void {
    if (confirm(`Remove mapping for "${mapping.samlAttributeValue}"?`)) {
      this.enterpriseService.deleteSamlRoleMapping(mapping.id).subscribe({
        next: () => this.loadRoleMappings(),
      });
    }
  }
}
