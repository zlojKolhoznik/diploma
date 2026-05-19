import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TablerIconComponent } from '@tabler/icons-angular';
import { ReservationsService } from '../../core/services/reservations.service';
import { ReservationResponse } from '../../core/models/reservation.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-waiter-reservations',
  standalone: true,
  imports: [CommonModule, FormsModule, SpinnerComponent, StatusBadgeComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>Reservations</h2>
        <input type="date" [(ngModel)]="selectedDate" (ngModelChange)="load()" class="date-filter" aria-label="Filter by date" />
      </div>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else if (reservations().length === 0) {
        <p class="empty-note">No reservations for this date.</p>
      } @else {
        <div class="table-wrapper">
          <table class="data-table">
            <thead>
              <tr>
                <th>Time</th>
                <th>Guest</th>
                <th>Guests</th>
                <th>Table</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (r of reservations(); track r.id) {
                <tr>
                  <td>{{ r.startTime | date:'shortTime' }}</td>
                  <td>{{ r.guestName ?? '—' }}</td>
                  <td>{{ r.numberOfGuests }}</td>
                  <td>{{ r.tableNumber ?? '—' }}</td>
                  <td><app-status-badge [status]="r.status ?? ''" type="reservation"></app-status-badge></td>
                  <td class="actions">
                    <button class="action-btn" (click)="updateStatus(r, 'Confirmed')" aria-label="Confirm">Confirm</button>
                    <button class="action-btn" (click)="updateStatus(r, 'Seated')" aria-label="Seat">Seat</button>
                    <button class="action-btn action-btn--danger" (click)="updateStatus(r, 'Cancelled')" aria-label="Cancel">Cancel</button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); }
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--space-xl); flex-wrap: wrap; gap: var(--space-md); }
    h2 { margin: 0; }
    .date-filter { padding: var(--space-sm) var(--space-md); border-radius: var(--radius-md); font-size: var(--text-sm); }
    .table-wrapper { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: var(--text-sm); }
    .data-table th, .data-table td { padding: var(--space-md); text-align: left; border-bottom: 1px solid var(--border-subtle); }
    .data-table th { color: var(--text-tertiary); font-weight: 600; background: var(--surface-800); }
    .data-table tr:hover td { background: var(--surface-700); }
    .actions { display: flex; gap: var(--space-xs); flex-wrap: wrap; }
    .action-btn {
      background: var(--surface-700); border: 1px solid var(--border-default);
      color: var(--text-secondary); padding: 2px var(--space-sm); border-radius: var(--radius-sm);
      font-size: var(--text-xs); cursor: pointer;
      &:hover { background: var(--surface-600); color: var(--text-primary); }
    }
    .action-btn--danger { border-color: var(--state-error); color: var(--state-error); &:hover { background: rgba(220,53,69,0.1); } }
    .empty-note { color: var(--text-secondary); padding: var(--space-xl); }
  `],
})
export class WaiterReservationsComponent implements OnInit {
  private readonly service = inject(ReservationsService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly reservations = signal<ReservationResponse[]>([]);
  selectedDate: string = new Date().toISOString().split('T')[0];

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getAll({ date: this.selectedDate }).subscribe({
      next: res => { this.reservations.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  updateStatus(r: ReservationResponse, status: string): void {
    this.service.updateStatus(r.id, { status }).subscribe({
      next: () => {
        this.reservations.update(list => list.map(x => x.id === r.id ? { ...x, status } : x));
        this.notifications.success(`Reservation marked as ${status}.`);
      },
      error: () => this.notifications.error('Failed to update status.'),
    });
  }
}

