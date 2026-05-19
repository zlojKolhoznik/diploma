import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TablerIconComponent } from '@tabler/icons-angular';
import { TablesService } from '../../core/services/tables.service';
import { RestaurantsService } from '../../core/services/restaurants.service';
import { TableBrief } from '../../core/models/table.models';
import { RestaurantBrief } from '../../core/models/restaurant.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-admin-tables',
  standalone: true,
  imports: [CommonModule, FormsModule, TablerIconComponent, SpinnerComponent, ConfirmDialogComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>Tables</h2>
        <select [(ngModel)]="selectedRestaurantId" (ngModelChange)="loadTables()" class="filter-select" aria-label="Select restaurant">
          <option value="">Select a restaurant</option>
          @for (r of restaurants(); track r.id) {
            <option [value]="r.id">{{ r.city }} | {{ r.address }}</option>
          }
        </select>
      </div>

      @if (loading()) { <app-spinner></app-spinner> }
      @else if (selectedRestaurantId) {
        <div class="tables-grid">
          @for (t of tables(); track t.tableNumber) {
            <div class="table-card">
              <div class="table-card__number">Table {{ t.tableNumber }}</div>
              <div class="table-card__seats"><tabler-icon [icon]="'users'" [size]="14"></tabler-icon> {{ t.seats }} seats</div>
              <button class="icon-btn icon-btn--danger" (click)="confirmDelete(t)" aria-label="Remove table"><tabler-icon [icon]="'trash'" [size]="14"></tabler-icon></button>
            </div>
          }
          <!-- Add new table card -->
          <div class="table-card table-card--add">
            <div class="form-row">
              <div class="field">
                <label for="new-table-number">Table number</label>
                <input id="new-table-number" type="number" [(ngModel)]="newTable.tableNumber" placeholder="No." min="1" aria-label="Table number" />
              </div>
              <div class="field">
                <label for="new-table-seats">Seats</label>
                <input id="new-table-seats" type="number" [(ngModel)]="newTable.seats" placeholder="Seats" min="1" aria-label="Seats" />
              </div>
            </div>
            <button class="btn-primary" (click)="addTable()" [disabled]="saving()" aria-label="Add table">
              <tabler-icon [icon]="'plus'" [size]="14"></tabler-icon> Add
            </button>
          </div>
        </div>
      }
      @else {
        <p class="empty-note">Select a restaurant to manage its tables.</p>
      }

      <app-confirm-dialog [open]="deletePending !== null" (confirmed)="doDelete()" (cancelled)="deletePending = null"></app-confirm-dialog>
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); max-width: 900px; }
    .page-header { display: flex; align-items: center; gap: var(--space-lg); margin-bottom: var(--space-xl); flex-wrap: wrap; }
    h2 { margin: 0; }
    .filter-select { padding: var(--space-sm) var(--space-md); border-radius: var(--radius-md); font-size: var(--text-sm); flex: 1; max-width: 360px; }
    .tables-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: var(--space-md); }
    .table-card {
      background: var(--surface-800); border: 1px solid var(--border-default); border-radius: var(--radius-md);
      padding: var(--space-md); display: flex; flex-direction: column; align-items: center; gap: var(--space-sm);
      text-align: center;
    }
    .table-card--add { border-style: dashed; }
    .table-card__number { font-weight: 700; font-size: var(--text-lg); }
    .table-card__seats { display: flex; align-items: center; gap: 4px; font-size: var(--text-sm); color: var(--text-secondary); }
    .icon-btn { background: var(--surface-700); border: 1px solid var(--border-default); color: var(--text-secondary); padding: 4px; border-radius: var(--radius-sm); cursor: pointer; display: flex; }
    .icon-btn--danger { &:hover { border-color: var(--state-error); color: var(--state-error); } }
    .form-row { display: flex; gap: var(--space-sm); width: 100%; margin-bottom: var(--space-xs); }
    .field { display: flex; flex-direction: column; gap: 6px; flex: 1; text-align: left; }
    .field label { font-size: var(--text-xs); color: var(--text-secondary); font-weight: 600; }
    .field input { width: 100%; padding: var(--space-xs) var(--space-sm); border-radius: var(--radius-sm); font-size: var(--text-sm); }
    .btn-primary { display: flex; align-items: center; gap: 4px; background: var(--accent-primary); color: white; border: none; padding: var(--space-xs) var(--space-md); border-radius: var(--radius-sm); cursor: pointer; font-size: var(--text-sm); &:hover { background: var(--accent-primary-hover); } &:disabled { opacity: 0.5; } }
    .empty-note { color: var(--text-secondary); padding: var(--space-lg) 0; }
  `],
})
export class AdminTablesComponent implements OnInit {
  private readonly tablesService = inject(TablesService);
  private readonly restaurantsService = inject(RestaurantsService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly tables = signal<TableBrief[]>([]);
  readonly restaurants = signal<RestaurantBrief[]>([]);

  selectedRestaurantId = '';
  newTable = { tableNumber: 0, seats: 2 };
  deletePending: TableBrief | null = null;

  ngOnInit(): void {
    this.restaurantsService.getAll().subscribe({ next: res => this.restaurants.set(res) });
  }

  loadTables(): void {
    if (!this.selectedRestaurantId) return;
    this.loading.set(true);
    this.tablesService.getByRestaurant(this.selectedRestaurantId).subscribe({
      next: res => { this.tables.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  addTable(): void {
    if (!this.selectedRestaurantId || !this.newTable.tableNumber) return;
    this.saving.set(true);
    this.tablesService.create(this.selectedRestaurantId, { tableNumber: this.newTable.tableNumber, seats: this.newTable.seats }).subscribe({
      next: t => { this.tables.update(list => [...list, t]); this.notifications.success('Table added.'); this.saving.set(false); this.newTable = { tableNumber: 0, seats: 2 }; },
      error: () => { this.notifications.error('Failed to add table.'); this.saving.set(false); },
    });
  }

  confirmDelete(t: TableBrief): void { this.deletePending = t; }

  doDelete(): void {
    const t = this.deletePending;
    if (!t) return;
    this.tablesService.delete(this.selectedRestaurantId, t.tableNumber).subscribe({
      next: () => { this.tables.update(list => list.filter(x => x.tableNumber !== t.tableNumber)); this.notifications.success('Table removed.'); this.deletePending = null; },
      error: () => { this.notifications.error('Delete failed.'); this.deletePending = null; },
    });
  }
}


