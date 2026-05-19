import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';
import { WaitersService } from '../../core/services/waiters.service';
import { WaiterResponse } from '../../core/models/waiter.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-admin-waiters',
  standalone: true,
  imports: [CommonModule, TablerIconComponent, SpinnerComponent, ConfirmDialogComponent],
  template: `
    <div class="page-container">
      <h2>Waiters</h2>

      @if (loading()) { <app-spinner></app-spinner> }
      @else {
        <div class="table-wrapper">
          <table class="data-table">
            <thead><tr><th>Name</th><th>Email</th><th>Restaurant</th><th>Actions</th></tr></thead>
            <tbody>
              @for (w of waiters(); track w.userId) {
                <tr>
                  <td>{{ w.firstName }} {{ w.lastName }}</td>
                  <td>{{ w.email }}</td>
                  <td>{{ w.restaurantId ?? '-' }}</td>
                  <td>
                    <button class="icon-btn icon-btn--danger" (click)="confirmDelete(w)" aria-label="Remove waiter">
                      <tabler-icon [icon]="'trash'" [size]="16"></tabler-icon>
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
          @if (waiters().length === 0) { <p class="empty-note">No waiters found.</p> }
        </div>
      }

      <app-confirm-dialog
        [open]="deleteId() !== null"
        title="Remove Waiter"
        message="This will remove the waiter role from this user."
        confirmLabel="Remove"
        (confirmed)="doDelete()"
        (cancelled)="deleteId.set(null)">
      </app-confirm-dialog>
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); max-width: 900px; }
    h2 { margin-bottom: var(--space-xl); }
    .table-wrapper { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: var(--text-sm); }
    .data-table th, .data-table td { padding: var(--space-md); text-align: left; border-bottom: 1px solid var(--border-subtle); }
    .data-table th { color: var(--text-tertiary); font-weight: 600; }
    .data-table tr:hover td { background: var(--surface-700); }
    .icon-btn { background: var(--surface-700); border: 1px solid var(--border-default); color: var(--text-secondary); padding: var(--space-xs); border-radius: var(--radius-sm); cursor: pointer; display: flex; }
    .icon-btn--danger { &:hover { border-color: var(--state-error); color: var(--state-error); } }
    .empty-note { color: var(--text-secondary); padding: var(--space-lg) 0; }
  `],
})
export class AdminWaitersComponent implements OnInit {
  private readonly service = inject(WaitersService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly waiters = signal<WaiterResponse[]>([]);
  readonly deleteId = signal<string | null>(null);

  ngOnInit(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: res => { this.waiters.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  confirmDelete(w: WaiterResponse): void { this.deleteId.set(w.userId); }

  doDelete(): void {
    const id = this.deleteId();
    if (!id) return;
    this.service.delete(id).subscribe({
      next: () => {
        this.waiters.update(list => list.filter(w => w.userId !== id));
        this.notifications.success('Waiter removed.'); this.deleteId.set(null);
      },
      error: () => { this.notifications.error('Delete failed.'); this.deleteId.set(null); },
    });
  }
}


