import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { User, UserRole } from '@core/models/user.model';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-agent-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="max-w-3xl mx-auto px-6 py-6">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h3 class="text-lg font-semibold text-gray-900">Agents</h3>
          <p class="text-sm text-gray-500">Manage your team members and their roles</p>
        </div>
        <button
          (click)="showInviteForm = !showInviteForm"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          {{ showInviteForm ? 'Cancel' : 'Invite Agent' }}
        </button>
      </div>

      <!-- Invite Form -->
      @if (showInviteForm) {
        <div class="mb-6 p-4 bg-white rounded-lg border border-gray-200">
          <h4 class="text-sm font-semibold text-gray-700 mb-4">Invite New Agent</h4>
          <form [formGroup]="inviteForm" (ngSubmit)="inviteAgent()" class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Name</label>
                <input
                  formControlName="name"
                  placeholder="Full name"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Email</label>
                <input
                  formControlName="email"
                  type="email"
                  placeholder="agent@company.com"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Role</label>
              <select
                formControlName="role"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white"
              >
                @for (role of roleOptions; track role.value) {
                  <option [value]="role.value">{{ role.label }}</option>
                }
              </select>
            </div>
            <div class="flex justify-end">
              <button
                type="submit"
                [disabled]="inviteForm.invalid"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                Send Invitation
              </button>
            </div>
          </form>
        </div>
      }

      <!-- Agents Table -->
      <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Agent</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Availability</th>
              <th class="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200">
            @for (agent of agents$ | async; track agent.id) {
              <tr class="hover:bg-gray-50">
                <td class="px-4 py-3">
                  <div class="flex items-center gap-3">
                    @if (agent.avatar) {
                      <img [src]="agent.avatar" class="h-8 w-8 rounded-full object-cover" alt="" />
                    } @else {
                      <div class="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center text-xs font-medium text-white">
                        {{ getInitials(agent.name) }}
                      </div>
                    }
                    <div>
                      <p class="text-sm font-medium text-gray-900">{{ agent.name }}</p>
                      <p class="text-xs text-gray-500">{{ agent.email }}</p>
                    </div>
                  </div>
                </td>
                <td class="px-4 py-3">
                  @if (editingAgentId === agent.id) {
                    <select
                      [value]="agent.role"
                      (change)="updateAgentRole(agent, $event)"
                      class="rounded-lg border border-gray-300 px-2 py-1 text-sm focus:border-blue-500 focus:outline-none bg-white"
                    >
                      @for (role of roleOptions; track role.value) {
                        <option [value]="role.value" [selected]="agent.role === role.value">{{ role.label }}</option>
                      }
                    </select>
                  } @else {
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                      [ngClass]="{
                        'bg-purple-100 text-purple-800': agent.role === 'administrator',
                        'bg-blue-100 text-blue-800': agent.role === 'supervisor',
                        'bg-gray-100 text-gray-800': agent.role === 'agent'
                      }"
                    >
                      {{ agent.role }}
                    </span>
                  }
                </td>
                <td class="px-4 py-3">
                  <div class="flex items-center gap-2">
                    <span
                      class="h-2.5 w-2.5 rounded-full"
                      [ngClass]="{
                        'bg-green-500': agent.availability === 'online',
                        'bg-yellow-500': agent.availability === 'busy',
                        'bg-gray-400': agent.availability === 'offline' || !agent.availability
                      }"
                    ></span>
                    <span class="text-sm text-gray-600">{{ agent.availability || 'offline' }}</span>
                  </div>
                </td>
                <td class="px-4 py-3 text-right">
                  <div class="flex items-center justify-end gap-2">
                    <button
                      (click)="toggleEdit(agent.id)"
                      class="p-1.5 text-gray-400 hover:text-blue-600 rounded-lg hover:bg-blue-50 transition-colors"
                      title="Edit"
                    >
                      <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0 1 15.75 21H5.25A2.25 2.25 0 0 1 3 18.75V8.25A2.25 2.25 0 0 1 5.25 6H10" />
                      </svg>
                    </button>
                    <button
                      (click)="deleteAgent(agent)"
                      class="p-1.5 text-gray-400 hover:text-red-600 rounded-lg hover:bg-red-50 transition-colors"
                      title="Remove"
                    >
                      <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="4" class="px-4 py-8 text-center text-sm text-gray-500">
                  No agents found. Invite your first team member to get started.
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class AgentListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);

  private readonly agentsSubject = new BehaviorSubject<User[]>([]);
  agents$ = this.agentsSubject.asObservable();

  showInviteForm = false;
  editingAgentId: number | null = null;

  roleOptions = [
    { value: UserRole.Agent, label: 'Agent' },
    { value: UserRole.Supervisor, label: 'Supervisor' },
    { value: UserRole.Administrator, label: 'Administrator' },
  ];

  inviteForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    role: [UserRole.Agent, Validators.required],
  });

  ngOnInit(): void {
    this.loadAgents();
  }

  loadAgents(): void {
    const path = this.api.accountPath('/agents');
    this.api.get<User[]>(path).subscribe((agents) => {
      this.agentsSubject.next(agents);
    });
  }

  inviteAgent(): void {
    if (this.inviteForm.invalid) return;
    const path = this.api.accountPath('/agents');
    this.api.post<User>(path, this.inviteForm.value).subscribe((agent) => {
      const current = this.agentsSubject.getValue();
      this.agentsSubject.next([...current, agent]);
      this.inviteForm.reset({ role: UserRole.Agent });
      this.showInviteForm = false;
    });
  }

  toggleEdit(agentId: number): void {
    this.editingAgentId = this.editingAgentId === agentId ? null : agentId;
  }

  updateAgentRole(agent: User, event: Event): void {
    const role = (event.target as HTMLSelectElement).value;
    const path = this.api.accountPath(`/agents/${agent.id}`);
    this.api.put<User>(path, { role }).subscribe(() => {
      const current = this.agentsSubject.getValue();
      const index = current.findIndex((a) => a.id === agent.id);
      if (index >= 0) {
        current[index] = { ...current[index], role };
        this.agentsSubject.next([...current]);
      }
      this.editingAgentId = null;
    });
  }

  deleteAgent(agent: User): void {
    if (!confirm(`Are you sure you want to remove ${agent.name}?`)) return;
    const path = this.api.accountPath(`/agents/${agent.id}`);
    this.api.delete(path).subscribe(() => {
      const current = this.agentsSubject.getValue();
      this.agentsSubject.next(current.filter((a) => a.id !== agent.id));
    });
  }

  getInitials(name: string): string {
    if (!name) return '';
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}
