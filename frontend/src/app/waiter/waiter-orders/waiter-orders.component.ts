import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';
import { OrdersService } from '../../core/services/orders.service';
import { OrderResponse } from '../../core/models/order.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { NotificationService } from '../../core/services/notification.service';

const STATUS_COLUMNS = ['Pending', 'InProgress', 'Ready', 'Delivered'] as const;

@Component({
  selector: 'app-waiter-orders',
  standalone: true,
  imports: [CommonModule, TablerIconComponent, SpinnerComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>Orders</h2>
        <button class="btn-refresh" (click)="load()" aria-label="Refresh orders">
          <tabler-icon [icon]="'refresh'\" [size]="16"></tabler-icon> Refresh
        </button>
      </div>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else {
        <div class="kanban">
          @for (col of columns; track col.status) {
            <div class="kanban-col">
              <div class="kanban-col__header">
                <h4>{{ col.label }}</h4>
                <span class="kanban-col__count">{{ col.orders().length }}</span>
              </div>
              @for (order of col.orders(); track order.id) {
                <div class="kanban-card">
                  <div class="kanban-card__meta">
                    <span>#{{ order.id.slice(-6) }}</span>
                    <span>\${{ order.totalAmount | number:'1.2-2' }}</span>
                  </div>
                  @if (order.items) {
                    <ul class="kanban-card__items">
                      @for (item of order.items; track item.id) {
                        <li>{{ item.quantity }}x {{ item.dishName }}</li>
                      }
                    </ul>
                  }
                  <div class="kanban-card__actions">
                    @if (col.status !== 'Delivered') {
                      <button class="action-btn" (click)="advance(order, col.status)" aria-label="Advance order status">
                        Next >
                      </button>
                    }
                  </div>
                </div>
              }
              @if (col.orders().length === 0) {
                <p class="kanban-empty">No orders</p>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); }
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--space-xl); }
    h2 { margin: 0; }
    .btn-refresh {
      display: flex; align-items: center; gap: var(--space-xs);
      background: var(--surface-700); border: 1px solid var(--border-default); color: var(--text-secondary);
      padding: var(--space-xs) var(--space-md); border-radius: var(--radius-md); cursor: pointer; font-size: var(--text-sm);
      &:hover { background: var(--surface-600); color: var(--text-primary); }
    }
    .kanban { display: grid; grid-template-columns: repeat(4, 1fr); gap: var(--space-lg); }
    .kanban-col { background: var(--surface-800); border: 1px solid var(--border-default); border-radius: var(--radius-lg); padding: var(--space-md); }
    .kanban-col__header { display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--space-md); }
    .kanban-col__header h4 { margin: 0; font-size: var(--text-sm); color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.05em; }
    .kanban-col__count { background: var(--surface-700); border-radius: var(--radius-full); width: 24px; height: 24px; display: flex; align-items: center; justify-content: center; font-size: var(--text-xs); font-weight: 700; }
    .kanban-card { background: var(--surface-700); border-radius: var(--radius-md); padding: var(--space-md); margin-bottom: var(--space-sm); }
    .kanban-card__meta { display: flex; justify-content: space-between; font-size: var(--text-xs); color: var(--text-tertiary); margin-bottom: var(--space-xs); }
    .kanban-card__items { list-style: none; padding: 0; margin: 0 0 var(--space-sm); font-size: var(--text-xs); color: var(--text-secondary); }
    .kanban-card__actions { display: flex; justify-content: flex-end; }
    .action-btn { background: var(--accent-primary); color: white; border: none; padding: 2px var(--space-sm); border-radius: var(--radius-sm); font-size: var(--text-xs); cursor: pointer; &:hover { opacity: 0.85; } }
    .kanban-empty { color: var(--text-tertiary); font-size: var(--text-sm); text-align: center; padding: var(--space-md) 0; }
    @media (max-width: 900px) { .kanban { grid-template-columns: repeat(2, 1fr); } }
    @media (max-width: 560px) { .kanban { grid-template-columns: 1fr; } }
  `],
})
export class WaiterOrdersComponent implements OnInit {
  private readonly service = inject(OrdersService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly allOrders = signal<OrderResponse[]>([]);

  readonly columns = STATUS_COLUMNS.map(status => ({
    status,
    label: this.labelFor(status),
    orders: computed(() => this.allOrders().filter(o => o.status === status)),
  }));

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: res => { this.allOrders.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  advance(order: OrderResponse, currentStatus: string): void {
    const next = this.nextStatus(currentStatus);
    if (!next) return;
    this.service.updateStatus(order.id, { status: next }).subscribe({
      next: () => {
        this.allOrders.update(list => list.map(o => o.id === order.id ? { ...o, status: next } : o));
        this.notifications.success(`Order moved to ${next}.`);
      },
      error: () => this.notifications.error('Failed to update order.'),
    });
  }

  private nextStatus(s: string): string | null {
    const map: Record<string, string> = { Pending: 'InProgress', InProgress: 'Ready', Ready: 'Delivered' };
    return map[s] ?? null;
  }

  private labelFor(s: string): string {
    const map: Record<string, string> = { Pending: 'Pending', InProgress: 'In Progress', Ready: 'Ready', Delivered: 'Delivered' };
    return map[s] ?? s;
  }
}


