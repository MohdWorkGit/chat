import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SuperAdminService } from '@core/services/super-admin.service';
import { User } from '@core/models/user.model';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center gap-3">
          <a routerLink="/super-admin" class="text-sm text-blue-600 hover:text-blue-800">&larr; Dashboard</a>
          <h2 class="text-lg font-semibold text-gray-900">All Users</h2>
        </div>
      </div>

      @if (loading) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else if (users.length > 0) {
        <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Account</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200">
              @for (user of users; track user.id) {
                <tr class="hover:bg-gray-50 transition-colors">
                  <td class="px-6 py-4 text-sm font-medium text-gray-900">{{ user.name || user.displayName }}</td>
                  <td class="px-6 py-4 text-sm text-gray-500">{{ user.email }}</td>
                  <td class="px-6 py-4 text-sm text-gray-500">{{ user.role || '—' }}</td>
                  <td class="px-6 py-4 text-sm text-gray-500">{{ user.accountId }}</td>
                  <td class="px-6 py-4 text-sm text-gray-500">{{ user.createdAt | date:'mediumDate' }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="bg-white rounded-lg border border-gray-200 text-center py-8">
          <p class="text-sm text-gray-500">No users found.</p>
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
export class AdminUsersComponent implements OnInit {
  private adminService = inject(SuperAdminService);

  users: User[] = [];
  loading = true;

  ngOnInit(): void {
    this.adminService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
