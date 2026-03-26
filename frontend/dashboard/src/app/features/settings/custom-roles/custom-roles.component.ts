import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EnterpriseService, CustomRole } from '@core/services/enterprise.service';

const AVAILABLE_PERMISSIONS = [
  { key: 'conversation_manage', label: 'Manage Conversations' },
  { key: 'contact_manage', label: 'Manage Contacts' },
  { key: 'report_manage', label: 'Manage Reports' },
  { key: 'knowledge_base_manage', label: 'Manage Knowledge Base' },
];

@Component({
  selector: 'app-custom-roles',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Custom Roles</h2>
          <p class="text-sm text-gray-500 mt-1">Define custom roles with granular permissions for your team.</p>
        </div>
        <button
          (click)="openCreateModal()"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Role
        </button>
      </div>

      <!-- Loading State -->
      @if (loading) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if (roles.length > 0) {
          <!-- Roles Table -->
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Permissions</th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                @for (role of roles; track role.id) {
                  <tr class="hover:bg-gray-50">
                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ role.name }}</td>
                    <td class="px-6 py-4 text-sm text-gray-500 max-w-xs truncate">{{ role.description || '-' }}</td>
                    <td class="px-6 py-4">
                      <div class="flex flex-wrap gap-1">
                        @for (perm of role.permissions; track perm) {
                          <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                            {{ formatPermission(perm) }}
                          </span>
                        }
                        @if (role.permissions.length === 0) {
                          <span class="text-xs text-gray-400">No permissions</span>
                        }
                      </div>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-right text-sm">
                      <button
                        (click)="openEditModal(role)"
                        class="text-blue-600 hover:text-blue-800 mr-3"
                      >
                        Edit
                      </button>
                      <button
                        (click)="deleteRole(role)"
                        class="text-red-600 hover:text-red-800"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        } @else {
          <!-- Empty State -->
          <div class="bg-white rounded-lg border border-gray-200 text-center py-12">
            <div class="mx-auto h-12 w-12 text-gray-400 mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
              </svg>
            </div>
            <h3 class="text-sm font-medium text-gray-900 mb-1">No custom roles yet</h3>
            <p class="text-xs text-gray-500 mb-4">Create custom roles to define granular permissions for your team members.</p>
            <button
              (click)="openCreateModal()"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
            >
              New Role
            </button>
          </div>
        }
      }

      <!-- Modal -->
      @if (showModal) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="fixed inset-0 bg-black/30" (click)="closeModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl w-full max-w-lg p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">
                {{ editingRole ? 'Edit Role' : 'New Role' }}
              </h3>
              <form [formGroup]="roleForm" (ngSubmit)="saveRole()" class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Name</label>
                  <input
                    formControlName="name"
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Role name"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
                  <input
                    formControlName="description"
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Brief description of this role"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Permissions</label>
                  <div class="space-y-2">
                    @for (perm of availablePermissions; track perm.key) {
                      <label class="flex items-center gap-2 cursor-pointer">
                        <input
                          type="checkbox"
                          [checked]="isPermissionSelected(perm.key)"
                          (change)="togglePermission(perm.key)"
                          class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                        />
                        <span class="text-sm text-gray-700">{{ perm.label }}</span>
                      </label>
                    }
                  </div>
                </div>
                <div class="flex justify-end gap-3 pt-2">
                  <button
                    type="button"
                    (click)="closeModal()"
                    class="px-4 py-2 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    [disabled]="roleForm.invalid"
                    class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    {{ editingRole ? 'Update' : 'Create' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
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
export class CustomRolesComponent implements OnInit {
  private enterpriseService = inject(EnterpriseService);
  private fb = inject(FormBuilder);

  roles: CustomRole[] = [];
  loading = true;
  showModal = false;
  editingRole: CustomRole | null = null;
  selectedPermissions: string[] = [];

  availablePermissions = AVAILABLE_PERMISSIONS;

  roleForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
  });

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.loading = true;
    this.enterpriseService.getCustomRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  formatPermission(permission: string): string {
    return permission
      .split('_')
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }

  isPermissionSelected(key: string): boolean {
    return this.selectedPermissions.includes(key);
  }

  togglePermission(key: string): void {
    const index = this.selectedPermissions.indexOf(key);
    if (index === -1) {
      this.selectedPermissions = [...this.selectedPermissions, key];
    } else {
      this.selectedPermissions = this.selectedPermissions.filter((p) => p !== key);
    }
  }

  openCreateModal(): void {
    this.editingRole = null;
    this.selectedPermissions = [];
    this.roleForm.reset({ name: '', description: '' });
    this.showModal = true;
  }

  openEditModal(role: CustomRole): void {
    this.editingRole = role;
    this.selectedPermissions = [...role.permissions];
    this.roleForm.patchValue({
      name: role.name,
      description: role.description,
    });
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingRole = null;
  }

  saveRole(): void {
    if (this.roleForm.invalid) return;

    const formValue = this.roleForm.value;

    if (this.editingRole) {
      this.enterpriseService
        .updateCustomRole(this.editingRole.id, {
          name: formValue.name,
          description: formValue.description,
          permissions: this.selectedPermissions,
        })
        .subscribe({
          next: () => {
            this.closeModal();
            this.loadRoles();
          },
        });
    } else {
      this.enterpriseService
        .createCustomRole({
          accountId: 0, // Server determines from auth context
          name: formValue.name,
          description: formValue.description,
          permissions: this.selectedPermissions,
        })
        .subscribe({
          next: () => {
            this.closeModal();
            this.loadRoles();
          },
        });
    }
  }

  deleteRole(role: CustomRole): void {
    if (confirm(`Are you sure you want to delete the "${role.name}" role?`)) {
      this.enterpriseService.deleteCustomRole(role.id).subscribe({
        next: () => this.loadRoles(),
      });
    }
  }
}
