import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { InboxesActions } from '@store/inboxes/inboxes.actions';
import { selectSelectedInbox, selectInboxesLoading } from '@store/inboxes/inboxes.selectors';
import { WorkingHour } from '@core/models/inbox.model';

@Component({
  selector: 'app-working-hours',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="p-6 max-w-3xl">
      <a routerLink="/settings/inboxes" class="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 mb-6">
        <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
        </svg>
        Back to Inboxes
      </a>

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if (inbox$ | async; as inbox) {
          <div class="mb-6">
            <h2 class="text-lg font-semibold text-gray-900">Working Hours — {{ inbox.name }}</h2>
            <p class="text-sm text-gray-500 mt-1">Configure business hours for this inbox. Messages outside these hours will receive an out-of-office reply.</p>
          </div>

          <!-- Timezone -->
          <div class="mb-6">
            <label class="block text-sm font-medium text-gray-700 mb-1">Timezone</label>
            <select
              [formControl]="timezoneControl"
              class="w-64 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              @for (tz of timezones; track tz) {
                <option [value]="tz">{{ tz }}</option>
              }
            </select>
          </div>

          <!-- Out of Office Message -->
          <div class="mb-6">
            <label class="block text-sm font-medium text-gray-700 mb-1">Out of Office Message</label>
            <textarea
              [formControl]="outOfOfficeControl"
              rows="3"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="We're currently offline. We'll get back to you during business hours."
            ></textarea>
          </div>

          <!-- Working Hours Table -->
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            <table class="w-full">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-32">Day</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-32">Status</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Open</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Close</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                @for (day of daysOfWeek; track day.value; let i = $index) {
                  <tr class="hover:bg-gray-50">
                    <td class="px-4 py-3 text-sm font-medium text-gray-900">{{ day.label }}</td>
                    <td class="px-4 py-3">
                      <select
                        [value]="getStatus(i)"
                        (change)="setDayStatus(i, $any($event.target).value)"
                        class="rounded-lg border border-gray-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      >
                        <option value="open">Custom</option>
                        <option value="open_all_day">Open 24h</option>
                        <option value="closed">Closed</option>
                      </select>
                    </td>
                    <td class="px-4 py-3">
                      @if (getStatus(i) === 'open') {
                        <div class="flex items-center gap-1">
                          <select
                            [value]="workingHours[i].openHour"
                            (change)="updateHour(i, 'openHour', $any($event.target).value)"
                            class="rounded border border-gray-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
                          >
                            @for (h of hours; track h) {
                              <option [value]="h">{{ h.toString().padStart(2, '0') }}</option>
                            }
                          </select>
                          <span class="text-gray-400">:</span>
                          <select
                            [value]="workingHours[i].openMinutes"
                            (change)="updateHour(i, 'openMinutes', $any($event.target).value)"
                            class="rounded border border-gray-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
                          >
                            @for (m of minutes; track m) {
                              <option [value]="m">{{ m.toString().padStart(2, '0') }}</option>
                            }
                          </select>
                        </div>
                      } @else {
                        <span class="text-sm text-gray-400">—</span>
                      }
                    </td>
                    <td class="px-4 py-3">
                      @if (getStatus(i) === 'open') {
                        <div class="flex items-center gap-1">
                          <select
                            [value]="workingHours[i].closeHour"
                            (change)="updateHour(i, 'closeHour', $any($event.target).value)"
                            class="rounded border border-gray-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
                          >
                            @for (h of hours; track h) {
                              <option [value]="h">{{ h.toString().padStart(2, '0') }}</option>
                            }
                          </select>
                          <span class="text-gray-400">:</span>
                          <select
                            [value]="workingHours[i].closeMinutes"
                            (change)="updateHour(i, 'closeMinutes', $any($event.target).value)"
                            class="rounded border border-gray-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
                          >
                            @for (m of minutes; track m) {
                              <option [value]="m">{{ m.toString().padStart(2, '0') }}</option>
                            }
                          </select>
                        </div>
                      } @else {
                        <span class="text-sm text-gray-400">—</span>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <!-- Save -->
          <div class="mt-6 flex gap-3">
            <button
              (click)="save()"
              class="px-6 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
            >
              Save Working Hours
            </button>
            <a
              routerLink="/settings/inboxes"
              class="px-6 py-2 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Cancel
            </a>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
      overflow-y: auto;
    }
  `],
})
export class WorkingHoursComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);

  inbox$ = this.store.select(selectSelectedInbox);
  loading$ = this.store.select(selectInboxesLoading);

  timezoneControl = this.fb.control('UTC');
  outOfOfficeControl = this.fb.control('');

  daysOfWeek = [
    { label: 'Monday', value: 1 },
    { label: 'Tuesday', value: 2 },
    { label: 'Wednesday', value: 3 },
    { label: 'Thursday', value: 4 },
    { label: 'Friday', value: 5 },
    { label: 'Saturday', value: 6 },
    { label: 'Sunday', value: 0 },
  ];

  hours = Array.from({ length: 24 }, (_, i) => i);
  minutes = [0, 15, 30, 45];

  timezones = [
    'UTC', 'US/Eastern', 'US/Central', 'US/Mountain', 'US/Pacific',
    'Europe/London', 'Europe/Paris', 'Europe/Berlin', 'Asia/Tokyo',
    'Asia/Shanghai', 'Asia/Kolkata', 'Australia/Sydney', 'Pacific/Auckland',
  ];

  workingHours: WorkingHour[] = this.daysOfWeek.map((d) => ({
    dayOfWeek: d.value,
    closedAllDay: d.value === 0 || d.value === 6,
    openAllDay: false,
    openHour: 9,
    openMinutes: 0,
    closeHour: 17,
    closeMinutes: 0,
  }));

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.store.dispatch(InboxesActions.loadInbox({ id }));
    }

    this.inbox$.subscribe((inbox) => {
      if (inbox) {
        this.timezoneControl.setValue(inbox.timezone || 'UTC');
        this.outOfOfficeControl.setValue(inbox.outOfOfficeMessage || '');
        if (inbox.workingHours?.length) {
          this.workingHours = [...inbox.workingHours];
        }
      }
    });
  }

  getStatus(index: number): string {
    const wh = this.workingHours[index];
    if (wh.closedAllDay) return 'closed';
    if (wh.openAllDay) return 'open_all_day';
    return 'open';
  }

  setDayStatus(index: number, status: string): void {
    this.workingHours[index] = {
      ...this.workingHours[index],
      closedAllDay: status === 'closed',
      openAllDay: status === 'open_all_day',
    };
  }

  updateHour(index: number, field: string, value: string): void {
    (this.workingHours[index] as any)[field] = Number(value);
  }

  save(): void {
    this.inbox$.subscribe((inbox) => {
      if (inbox) {
        this.store.dispatch(InboxesActions.updateInbox({
          id: inbox.id,
          data: {
            timezone: this.timezoneControl.value ?? 'UTC',
            outOfOfficeMessage: this.outOfOfficeControl.value ?? '',
            workingHours: this.workingHours,
          },
        }));
      }
    }).unsubscribe();
  }
}
