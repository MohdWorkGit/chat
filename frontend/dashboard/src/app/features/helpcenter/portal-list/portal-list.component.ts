import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HelpCenterService } from '@core/services/helpcenter.service';
import { Portal } from '@core/models/helpcenter.model';
import { HelpCenterTabsComponent } from '../helpcenter-tabs/helpcenter-tabs.component';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-portal-list',
  standalone: true,
  imports: [CommonModule, RouterLink, HelpCenterTabsComponent],
  template: `
    <app-helpcenter-tabs />
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Help Center Portals</h2>
        <a
          routerLink="new"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Portal
        </a>
      </div>

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((portals$ | async); as portals) {
          @if (portals.length === 0) {
            <div class="text-center py-12">
              <p class="text-sm text-gray-500">No portals yet.</p>
              <a
                routerLink="new"
                class="mt-4 inline-block px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-700"
              >
                Create your first portal
              </a>
            </div>
          } @else {
            <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Slug</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Custom Domain</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                    <th class="px-6 py-3"></th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                  @for (portal of portals; track portal.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
                      <td class="px-6 py-4">
                        <a [routerLink]="[portal.id]" class="text-sm font-medium text-gray-900 hover:text-blue-600">
                          {{ portal.name }}
                        </a>
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ portal.slug }}</td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ portal.customDomain || '—' }}</td>
                      <td class="px-6 py-4">
                        <span
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          [class]="portal.archived ? 'bg-gray-100 text-gray-700' : 'bg-green-100 text-green-800'"
                        >
                          {{ portal.archived ? 'Archived' : 'Active' }}
                        </span>
                      </td>
                      <td class="px-6 py-4 text-right">
                        <button
                          (click)="deletePortal(portal)"
                          class="text-sm text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
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
    :host { display: block; height: 100%; overflow-y: auto; }
  `],
})
export class PortalListComponent implements OnInit {
  private readonly helpCenter = inject(HelpCenterService);

  portals$ = new BehaviorSubject<Portal[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);

  ngOnInit(): void {
    this.loadPortals();
  }

  private loadPortals(): void {
    this.loading$.next(true);
    this.helpCenter.getPortals().subscribe({
      next: (portals) => {
        this.portals$.next(portals ?? []);
        this.loading$.next(false);
      },
      error: () => this.loading$.next(false),
    });
  }

  deletePortal(portal: Portal): void {
    if (!confirm(`Delete portal "${portal.name}"? This removes all categories and articles inside it.`)) return;
    this.helpCenter.deletePortal(portal.id).subscribe({
      next: () => this.loadPortals(),
    });
  }
}
