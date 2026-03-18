import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { TeamsActions } from '@store/teams/teams.actions';
import { selectAllTeams, selectTeamsLoading } from '@store/teams/teams.selectors';

@Component({
  selector: 'app-team-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Teams</h2>
        <a
          routerLink="new"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          Create Team
        </a>
      </div>

      <!-- Loading State -->
      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((teams$ | async); as teams) {
          @if (teams.length === 0) {
            <div class="text-center py-12">
              <svg class="mx-auto h-12 w-12 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M18 18.72a9.094 9.094 0 0 0 3.741-.479 3 3 0 0 0-4.682-2.72m.94 3.198.001.031c0 .225-.012.447-.037.666A11.944 11.944 0 0 1 12 21c-2.17 0-4.207-.576-5.963-1.584A6.062 6.062 0 0 1 6 18.719m12 0a5.971 5.971 0 0 0-.941-3.197m0 0A5.995 5.995 0 0 0 12 12.75a5.995 5.995 0 0 0-5.058 2.772m0 0a3 3 0 0 0-4.681 2.72 8.986 8.986 0 0 0 3.74.477m.94-3.197a5.971 5.971 0 0 0-.94 3.197M15 6.75a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm6 3a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Zm-13.5 0a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Z" />
              </svg>
              <p class="mt-2 text-sm text-gray-500">No teams created yet.</p>
              <a
                routerLink="new"
                class="mt-4 inline-block px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-700"
              >
                Create your first team
              </a>
            </div>
          } @else {
            <!-- Teams Table -->
            <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Auto Assign</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Members</th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                  @for (team of teams; track team.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
                      <td class="px-6 py-4">
                        <a [routerLink]="[team.id]" class="text-sm font-medium text-gray-900 hover:text-blue-600">
                          {{ team.name }}
                        </a>
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500 max-w-xs truncate">
                        {{ team.description || '--' }}
                      </td>
                      <td class="px-6 py-4">
                        <span
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          [class]="team.allowAutoAssign ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'"
                        >
                          {{ team.allowAutoAssign ? 'Enabled' : 'Disabled' }}
                        </span>
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500">
                        {{ team.members?.length || 0 }} members
                      </td>
                      <td class="px-6 py-4 text-right">
                        <a
                          [routerLink]="[team.id]"
                          class="text-sm text-blue-600 hover:text-blue-800 font-medium"
                        >
                          Settings
                        </a>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
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
export class TeamListComponent implements OnInit {
  private store = inject(Store);

  teams$ = this.store.select(selectAllTeams);
  loading$ = this.store.select(selectTeamsLoading);

  ngOnInit(): void {
    this.store.dispatch(TeamsActions.loadTeams());
  }
}
