import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TablerIconComponent } from '@tabler/icons-angular';
import { WaiterScheduleService } from '../../core/services/waiter-schedule.service';
import { WaitersService } from '../../core/services/waiters.service';
import { WaiterScheduleResponse, CreateWaiterScheduleRequest } from '../../core/models/schedule.models';
import { WaiterResponse } from '../../core/models/waiter.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-admin-schedules',
  standalone: true,
  imports: [CommonModule, FormsModule, TablerIconComponent, SpinnerComponent, ModalComponent, ConfirmDialogComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>Schedules</h2>
        <div class="header-actions">
          <select [(ngModel)]="selectedWaiterId" (ngModelChange)="loadSchedules()" class="filter-select" aria-label="Select waiter">
            <option value="">Select a waiter</option>
            @for (w of waiters(); track w.userId) {
              <option [value]="w.userId">{{ w.firstName }} {{ w.lastName }}</option>
            }
          </select>
          <button class="btn-primary" (click)="openCreate()" aria-label="Add schedule"><tabler-icon [icon]="'plus'" [size]="16"></tabler-icon> Add</button>
        </div>
      </div>

      @if (loading()) { <app-spinner></app-spinner> }
      @else if (selectedWaiterId) {
        <div class="table-wrapper">
          <table class="data-table">
            <thead><tr><th>Date</th><th>Shift Start</th><th>Shift End</th><th>Actions</th></tr></thead>
            <tbody>
              @for (s of schedules(); track s.id) {
                <tr>
                  <td>{{ formatDate(s) }}</td>
                  <td>{{ formatTime(s.shiftStart) }}</td>
                  <td>{{ formatTime(s.shiftEnd) }}</td>
                  <td>
                    <button class="icon-btn icon-btn--danger" (click)="confirmDelete(s)" aria-label="Delete schedule"><tabler-icon [icon]="'trash'" [size]="16"></tabler-icon></button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
          @if (schedules().length === 0) { <p class="empty-note">No schedules for this waiter.</p> }
        </div>
      } @else {
        <p class="empty-note">Select a waiter to view their schedule.</p>
      }

      <app-modal title="Add Schedule" [open]="showForm()" (closed)="showForm.set(false)">
        <form (ngSubmit)="saveSchedule()" class="form">
          <div class="form-group"><label for="sdate">Date *</label><input id="sdate" type="date" [(ngModel)]="formDate" name="date" required /></div>
          <div class="form-group"><label for="sstart">Shift Start *</label><input id="sstart" type="time" [(ngModel)]="formStart" name="shiftStart" required /></div>
          <div class="form-group"><label for="send">Shift End *</label><input id="send" type="time" [(ngModel)]="formEnd" name="shiftEnd" required /></div>
          <div class="form-actions">
            <button type="button" class="btn-ghost" (click)="showForm.set(false)">Cancel</button>
            <button type="submit" class="btn-primary" [disabled]="saving()">{{ saving() ? 'Saving...' : 'Save' }}</button>
          </div>
        </form>
      </app-modal>

      <app-confirm-dialog [open]="deleteId() !== null" (confirmed)="doDelete()" (cancelled)="deleteId.set(null)"></app-confirm-dialog>
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); max-width: 900px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-xl); flex-wrap: wrap; gap: var(--space-md); }
    h2 { margin: 0; }
    .header-actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }
    .filter-select { padding: var(--space-sm) var(--space-md); border-radius: var(--radius-md); font-size: var(--text-sm); }
    .table-wrapper { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: var(--text-sm); }
    .data-table th, .data-table td { padding: var(--space-md); text-align: left; border-bottom: 1px solid var(--border-subtle); }
    .data-table th { color: var(--text-tertiary); font-weight: 600; }
    .data-table tr:hover td { background: var(--surface-700); }
    .icon-btn { background: var(--surface-700); border: 1px solid var(--border-default); color: var(--text-secondary); padding: var(--space-xs); border-radius: var(--radius-sm); cursor: pointer; display: flex; }
    .icon-btn--danger { &:hover { border-color: var(--state-error); color: var(--state-error); } }
    .btn-primary { display: inline-flex; align-items: center; gap: var(--space-xs); background: var(--accent-primary); color: white; border: none; padding: var(--space-sm) var(--space-lg); border-radius: var(--radius-md); font-weight: 600; cursor: pointer; font-size: var(--text-sm); &:hover { background: var(--accent-primary-hover); } &:disabled { opacity: 0.5; } }
    .btn-ghost { background: transparent; border: 1px solid var(--border-default); color: var(--text-secondary); padding: var(--space-sm) var(--space-lg); border-radius: var(--radius-md); cursor: pointer; font-size: var(--text-sm); }
    .form { display: flex; flex-direction: column; gap: var(--space-lg); }
    .form-group { display: flex; flex-direction: column; gap: var(--space-xs); label { font-size: var(--text-sm); font-weight: 500; color: var(--text-secondary); } }
    .form-actions { display: flex; gap: var(--space-sm); justify-content: flex-end; }
    .empty-note { color: var(--text-secondary); padding: var(--space-lg) 0; }
  `],
})
export class AdminSchedulesComponent implements OnInit {
  private readonly scheduleService = inject(WaiterScheduleService);
  private readonly waitersService = inject(WaitersService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly schedules = signal<WaiterScheduleResponse[]>([]);
  readonly waiters = signal<WaiterResponse[]>([]);
  readonly showForm = signal(false);
  readonly deleteId = signal<string | null>(null);

  selectedWaiterId = '';
  formDate = '';
  formStart = '09:00';
  formEnd = '17:00';

  ngOnInit(): void {
    this.waitersService.getAll().subscribe({ next: res => this.waiters.set(res) });
  }

  loadSchedules(): void {
    if (!this.selectedWaiterId) return;
    this.loading.set(true);
    this.scheduleService.getAll(this.selectedWaiterId).subscribe({
      next: res => { this.schedules.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.formDate = ''; this.formStart = '09:00'; this.formEnd = '17:00'; this.showForm.set(true); }

  saveSchedule(): void {
    if (!this.selectedWaiterId || !this.formDate) return;
    const [year, month, day] = this.formDate.split('-').map(Number);
    const [sh, sm] = this.formStart.split(':').map(Number);
    const [eh, em] = this.formEnd.split(':').map(Number);
    const req: CreateWaiterScheduleRequest = {
      waiterId: this.selectedWaiterId,
      date: { year, month, day },
      shiftStart: { hour: sh, minute: sm },
      shiftEnd: { hour: eh, minute: em },
    };
    this.saving.set(true);
    this.scheduleService.create(this.selectedWaiterId, req).subscribe({
      next: () => { this.saving.set(false); this.showForm.set(false); this.notifications.success('Schedule created.'); this.loadSchedules(); },
      error: () => { this.saving.set(false); this.notifications.error('Failed to create schedule.'); },
    });
  }

  confirmDelete(s: WaiterScheduleResponse): void { this.deleteId.set(s.id); }

  doDelete(): void {
    const id = this.deleteId();
    if (!id || !this.selectedWaiterId) return;
    this.scheduleService.delete(this.selectedWaiterId, id).subscribe({
      next: () => { this.schedules.update(list => list.filter(s => s.id !== id)); this.notifications.success('Schedule deleted.'); this.deleteId.set(null); },
      error: () => { this.notifications.error('Delete failed.'); this.deleteId.set(null); },
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


