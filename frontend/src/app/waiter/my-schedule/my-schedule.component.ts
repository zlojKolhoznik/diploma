import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';
import { WaiterScheduleService } from '../../core/services/waiter-schedule.service';
import { AuthService } from '../../core/services/auth.service';
import { WaiterScheduleResponse } from '../../core/models/schedule.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-my-schedule',
  standalone: true,
  imports: [CommonModule, TablerIconComponent, SpinnerComponent],
  template: `
    <div class="page-container">
      <h2>My Schedule</h2>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else if (schedules().length === 0) {
        <div class="empty-state">
          <tabler-icon [icon]="'clock'" [size]="48"></tabler-icon>
          <h3>No schedule found</h3>
          <p>Your upcoming shifts will appear here.</p>
        </div>
      } @else {
        <div class="schedule-list">
          @for (s of schedules(); track s.id) {
            <div class="schedule-row">
              <div class="schedule-date">
                <tabler-icon [icon]="'calendar'" [size]="16"></tabler-icon>
                {{ formatDate(s) }}
              </div>
              <div class="schedule-shift">
                <tabler-icon [icon]="'clock'" [size]="16"></tabler-icon>
                {{ formatTime(s.shiftStart) }} | {{ formatTime(s.shiftEnd) }}
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); }
    h2 { margin-bottom: var(--space-xl); }
    .schedule-list { display: flex; flex-direction: column; gap: var(--space-sm); }
    .schedule-row {
      display: flex; align-items: center; gap: var(--space-xl);
      background: var(--surface-800); border: 1px solid var(--border-default);
      border-radius: var(--radius-md); padding: var(--space-md) var(--space-lg);
    }
    .schedule-date, .schedule-shift { display: flex; align-items: center; gap: var(--space-xs); font-size: var(--text-sm); color: var(--text-secondary); }
    .schedule-date { font-weight: 600; color: var(--text-primary); min-width: 160px; }
    .empty-state { text-align: center; padding: var(--space-2xl); color: var(--text-tertiary); h3, p { margin: var(--space-sm) 0; } }
  `],
})
export class MyScheduleComponent implements OnInit {
  private readonly service = inject(WaiterScheduleService);
  private readonly auth = inject(AuthService);

  readonly loading = signal(false);
  readonly schedules = signal<WaiterScheduleResponse[]>([]);

  ngOnInit(): void {
    const userId = this.auth.currentUser()?.sub;
    if (!userId) return;
    this.loading.set(true);
    this.service.getAll(userId).subscribe({
      next: res => { this.schedules.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  formatDate(s: WaiterScheduleResponse): string {
    const { year, month, day } = s.date;
    return new Date(year, month - 1, day).toLocaleDateString(undefined, { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' });
  }

  formatTime(t: { hour: number; minute: number }): string {
    return `${String(t.hour).padStart(2, '0')}:${String(t.minute).padStart(2, '0')}`;
  }
}


