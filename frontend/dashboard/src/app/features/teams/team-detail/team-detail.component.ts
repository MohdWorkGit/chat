import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TeamsActions } from '@store/teams/teams.actions';
import { selectSelectedTeam, selectTeamsLoading } from '@store/teams/teams.selectors';

@Component({
  selector: 'app-team-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <!-- Back Link -->
      <a routerLink="/settings/teams" class="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 mb-6">
        <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
        </svg>
        Back to Teams
      </a>

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if (team$ | async; as team) {
          <div class="max-w-2xl">
            <h2 class="text-lg font-semibold text-gray-900 mb-6">{{ team.name }} Settings</h2>

            <form [formGroup]="teamForm" (ngSubmit)="save()" class="space-y-6">
              <!-- Name -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Team Name</label>
                <input
                  formControlName="name"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>

              <!-- Description -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  formControlName="description"
                  rows="3"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="A brief description of this team..."
                ></textarea>
              </div>

              <!-- Auto Assign Toggle -->
              <div class="flex items-center justify-between">
                <div>
                  <span class="text-sm font-medium text-gray-700">Allow Auto Assign</span>
                  <p class="text-xs text-gray-400">Automatically assign conversations to team members</p>
                </div>
                <div class="relative">
                  <input
                    type="checkbox"
                    formControlName="allowAutoAssign"
                    class="sr-only peer"
                    id="allow-auto-assign"
                  />
                  <label
                    for="allow-auto-assign"
                    class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                    [class]="teamForm.get('allowAutoAssign')?.value ? 'bg-blue-600' : 'bg-gray-300'"
                  >
                    <span
                      class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                      [class]="teamForm.get('allowAutoAssign')?.value ? 'translate-x-4.5 ml-0.5' : 'translate-x-0.5'"
                    ></span>
                  </label>
                </div>
              </div>

              <!-- Members Section -->
              <div class="border-t border-gray-200 pt-6">
                <div class="flex items-center justify-between mb-4">
                  <h3 class="text-sm font-semibold text-gray-900">Members</h3>
                  <button
                    type="button"
                    (click)="addMember()"
                    class="px-3 py-1.5 text-xs font-medium text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 transition-colors"
                  >
                    Add Member
                  </button>
                </div>
                @if (team.members && team.members.length > 0) {
                  <ul class="space-y-3">
                    @for (member of team.members; track member.id) {
                      <li class="flex items-center justify-between py-2">
                        <div class="flex items-center gap-3">
                          @if (member.userAvatar) {
                            <img [src]="member.userAvatar" class="h-8 w-8 rounded-full object-cover" />
                          } @else {
                            <div class="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center text-xs font-medium text-white">
                              {{ member.userName?.charAt(0)?.toUpperCase() || '?' }}
                            </div>
                          }
                          <span class="text-sm text-gray-700">{{ member.userName || 'Agent #' + member.userId }}</span>
                        </div>
                        <button
                          type="button"
                          (click)="removeMember(member.id)"
                          class="text-sm text-red-600 hover:text-red-800"
                        >
                          Remove
                        </button>
                      </li>
                    }
                  </ul>
                } @else {
                  <p class="text-sm text-gray-500">No members in this team.</p>
                }
              </div>

              <!-- Actions -->
              <div class="flex items-center justify-between pt-6 border-t border-gray-200">
                <button
                  type="button"
                  (click)="deleteTeam()"
                  class="px-4 py-2 text-sm font-medium text-red-600 border border-red-300 rounded-lg hover:bg-red-50 transition-colors"
                >
                  Delete Team
                </button>
                <button
                  type="submit"
                  [disabled]="teamForm.invalid || teamForm.pristine"
                  class="px-6 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  Save Changes
                </button>
              </div>
            </form>
          </div>
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
export class TeamDetailComponent implements OnInit {
  @Input() id!: string;

  private store = inject(Store);
  private fb = inject(FormBuilder);

  team$ = this.store.select(selectSelectedTeam);
  loading$ = this.store.select(selectTeamsLoading);

  teamForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    allowAutoAssign: [false],
  });

  ngOnInit(): void {
    this.store.dispatch(TeamsActions.loadTeam({ id: Number(this.id) }));

    this.team$.subscribe((team) => {
      if (team) {
        this.teamForm.patchValue({
          name: team.name,
          description: team.description || '',
          allowAutoAssign: team.allowAutoAssign,
        });
      }
    });
  }

  save(): void {
    if (this.teamForm.invalid) return;
    this.store.dispatch(
      TeamsActions.updateTeam({
        id: Number(this.id),
        data: this.teamForm.value,
      })
    );
    this.teamForm.markAsPristine();
  }

  deleteTeam(): void {
    if (confirm('Are you sure you want to delete this team? This action cannot be undone.')) {
      this.store.dispatch(TeamsActions.deleteTeam({ id: Number(this.id) }));
    }
  }

  addMember(): void {
    // In a real app, this would open a dialog to select a user
    this.store.dispatch(TeamsActions.addMember({ teamId: Number(this.id), userId: 0 }));
  }

  removeMember(memberId: number): void {
    this.store.dispatch(
      TeamsActions.removeMember({ teamId: Number(this.id), memberId })
    );
  }
}
