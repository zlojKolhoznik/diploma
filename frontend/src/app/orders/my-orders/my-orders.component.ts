import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TablerIconComponent } from '@tabler/icons-angular';
import { OrdersService } from '../../core/services/orders.service';
import { OrderResponse } from '../../core/models/order.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule, TablerIconComponent, SpinnerComponent, StatusBadgeComponent],
  template: `
    <div class="page-container">
      <h2>My Orders</h2>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else if (orders().length === 0) {
        <div class="empty-state">
          <tabler-icon [icon]="'utensils-fork'" [size]="48"></tabler-icon>
          <h3>No orders yet</h3>
          <p>Orders placed at restaurants will appear here.</p>
        </div>
      } @else {
        <div class="orders-list">
          @for (order of orders(); track order.id) {
            <div class="order-card">
              <div class="order-card__header">
                <div>
                  <div class="order-card__time">
                    <tabler-icon [icon]="'clock'" [size]="14"></tabler-icon>
                    {{ order.createdAtUtc | date:'medium' }}
                  </div>
                </div>
                <div class="order-card__right">
                  <app-status-badge [status]="order.status ?? ''" type="order"></app-status-badge>
                  <span class="order-card__total">\${{ order.totalAmount | number:'1.2-2' }}</span>
                </div>
              </div>
              @if (order.items && order.items.length > 0) {
                <ul class="order-items">
                  @for (item of order.items; track item.id) {
                    <li class="order-item">
                      <span>{{ item.dishName }}</span>
                      <span>x{{ item.quantity }}</span>
                      <span>\${{ item.lineTotal | number:'1.2-2' }}</span>
                    </li>
                  }
                </ul>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { max-width: 800px; margin: 0 auto; padding: var(--space-xl) var(--space-lg); }
    h2 { margin-bottom: var(--space-xl); }
    .orders-list { display: flex; flex-direction: column; gap: var(--space-md); }
    .order-card { background: var(--surface-800); border: 1px solid var(--border-default); border-radius: var(--radius-lg); padding: var(--space-lg); }
    .order-card__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-md); }
    .order-card__time { display: flex; align-items: center; gap: var(--space-xs); font-size: var(--text-sm); color: var(--text-secondary); }
    .order-card__right { display: flex; align-items: center; gap: var(--space-md); }
    .order-card__total { font-weight: 700; color: var(--accent-primary); }
    .order-items { list-style: none; padding: 0; margin: 0; border-top: 1px solid var(--border-subtle); padding-top: var(--space-sm); }
    .order-item { display: flex; justify-content: space-between; font-size: var(--text-sm); color: var(--text-secondary); padding: var(--space-xs) 0; }
    .empty-state { text-align: center; padding: var(--space-2xl); color: var(--text-tertiary); h3, p { margin: var(--space-sm) 0; } }
  `],
})
export class MyOrdersComponent implements OnInit {
  private readonly service = inject(OrdersService);

  readonly loading = signal(false);
  readonly orders = signal<OrderResponse[]>([]);

  ngOnInit(): void {
    this.loading.set(true);
    this.service.getMy().subscribe({
      next: res => { this.orders.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}


