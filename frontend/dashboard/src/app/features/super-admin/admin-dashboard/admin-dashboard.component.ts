import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SuperAdminService, Account, AdminStats } from '@core/services/super-admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Super Admin Dashboard</h2>
      </div>

      <!-- Loading State -->
      @if (loading) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        <!-- Summary Cards -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          <div class="bg-white rounded-lg border border-gray-200 p-5">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-xs font-medium text-gray-500 uppercase tracking-wider">Total Accounts</p>
                <p class="mt-1 text-2xl font-semibold text-gray-900">{{ stats?.totalAccounts ?? 0 }}</p>
              </div>
              <div class="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 21h16.5M4.5 3h15M5.25 3v18m13.5-18v18M9 6.75h1.5m-1.5 3h1.5m-1.5 3h1.5m3-6H15m-1.5 3H15m-1.5 3H15M9 21v-3.375c0-.621.504-1.125 1.125-1.125h3.75c.621 0 1.125.504 1.125 1.125V21" />
                </svg>
              </div>
            </div>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-5">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-xs font-medium text-gray-500 uppercase tracking-wider">Total Users</p>
                <p class="mt-1 text-2xl font-semibold text-gray-900">{{ stats?.totalUsers ?? 0 }}</p>
              </div>
              <div class="h-10 w-10 rounded-full bg-green-100 flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z" />
                </svg>
              </div>
            </div>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-5">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-xs font-medium text-gray-500 uppercase tracking-wider">Active Conversations</p>
                <p class="mt-1 text-2xl font-semibold text-gray-900">{{ stats?.activeConversations ?? 0 }}</p>
              </div>
              <div class="h-10 w-10 rounded-full bg-purple-100 flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-purple-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M20.25 8.511c.884.284 1.5 1.128 1.5 2.097v4.286c0 1.136-.847 2.1-1.98 2.193-.34.027-.68.052-1.02.072v3.091l-3-3c-1.354 0-2.694-.055-4.02-.163a2.115 2.115 0 01-.825-.242m9.345-8.334a2.126 2.126 0 00-.476-.095 48.64 48.64 0 00-8.048 0c-1.131.094-1.976 1.057-1.976 2.192v4.286c0 .837.46 1.58 1.155 1.951m9.345-8.334V6.637c0-1.621-1.152-3.026-2.76-3.235A48.455 48.455 0 0011.25 3c-2.115 0-4.198.137-6.24.402-1.608.209-2.76 1.614-2.76 3.235v6.226c0 1.621 1.152 3.026 2.76 3.235.577.075 1.157.14 1.74.194V21l4.155-4.155" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="mb-8">
          <h3 class="text-sm font-medium text-gray-900 mb-3">Quick Actions</h3>
          <div class="flex gap-3">
            <button
              (click)="createAccount()"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
            >
              Create Account
            </button>
            <button
              (click)="manageUsers()"
              class="px-4 py-2 text-sm font-medium text-gray-700 border border-gray-300 hover:bg-gray-50 rounded-lg transition-colors"
            >
              Manage Users
            </button>
          </div>
        </div>

        <!-- Accounts Table -->
        <div>
          <h3 class="text-sm font-medium text-gray-900 mb-3">Accounts</h3>
          @if (accounts.length > 0) {
            <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Users</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                  @for (account of accounts; track account.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
                      <td class="px-6 py-4 text-sm font-medium text-gray-900">{{ account.name }}</td>
                      <td class="px-6 py-4">
                        <span
                          class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
                          [ngClass]="{
                            'bg-green-100 text-green-800': account.status === 'active',
                            'bg-red-100 text-red-800': account.status === 'suspended',
                            'bg-gray-100 text-gray-800': account.status !== 'active' && account.status !== 'suspended'
                          }"
                        >
                          {{ account.status }}
                        </span>
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ account.createdAt | date:'mediumDate' }}</td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ account.userCount }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          } @else {
            <div class="bg-white rounded-lg border border-gray-200 text-center py-8">
              <p class="text-sm text-gray-500">No accounts found.</p>
            </div>
          }
        </div>
      }

      @if (showCreateModal) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="fixed inset-0 bg-black/30" (click)="closeCreateModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl w-full max-w-md p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Create Account</h3>
              <form [formGroup]="createForm" (ngSubmit)="submitCreate()" class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Account Name</label>
                  <input
                    formControlName="name"
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Acme Corp"
                  />
                </div>
                @if (createError) {
                  <p class="text-xs text-red-600">{{ createError }}</p>
                }
                <div class="flex justify-end gap-3 pt-2">
                  <button
                    type="button"
                    (click)="closeCreateModal()"
                    class="px-4 py-2 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    [disabled]="createForm.invalid || creating"
                    class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    {{ creating ? 'Creating…' : 'Create' }}
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
export class AdminDashboardComponent implements OnInit {
  private adminService = inject(SuperAdminService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  accounts: Account[] = [];
  stats: AdminStats | null = null;
  loading = true;

  showCreateModal = false;
  creating = false;
  createError: string | null = null;
  createForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;

    this.adminService.getStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
    });

    this.adminService.getAccounts().subscribe({
      next: (accounts) => {
        this.accounts = accounts;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  createAccount(): void {
    this.createError = null;
    this.createForm.reset({ name: '' });
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.creating = false;
  }

  submitCreate(): void {
    if (this.createForm.invalid || this.creating) return;
    this.creating = true;
    this.createError = null;
    const name = this.createForm.value.name as string;
    this.adminService.createAccount(name).subscribe({
      next: () => {
        this.closeCreateModal();
        this.loadData();
      },
      error: (err) => {
        this.creating = false;
        this.createError = err?.error?.message ?? 'Failed to create account.';
      },
    });
  }

  manageUsers(): void {
    this.router.navigate(['/super-admin/users']);
  }
}
