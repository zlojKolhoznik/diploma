import { ChangeDetectionStrategy, Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TablerIconComponent } from '@tabler/icons-angular';
import { ReservationsService } from '../../core/services/reservations.service';
import { ReservationResponse } from '../../core/models/reservation.models';
import { RestaurantBrief } from '../../core/models/restaurant.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { NotificationService } from '../../core/services/notification.service';
import { RestaurantsService } from '../../core/services/restaurants.service';

@Component({
  selector: 'app-my-reservations',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, RouterModule, TablerIconComponent, SpinnerComponent, StatusBadgeComponent, ConfirmDialogComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h2>My Reservations</h2>
        <a class="btn btn--primary" routerLink="/reservations/new">Create reservation</a>
      </div>

      @if (loading()) {
        <app-spinner></app-spinner>
      } @else if (reservations().length === 0) {
        <div class="empty-state">
          <tabler-icon [icon]="'calendar'" [size]="48"></tabler-icon>
          <h3>No reservations yet</h3>
          <p>Book a table at a <a routerLink="/restaurants">restaurant</a> or create directly.</p>
          <a class="btn btn--primary" routerLink="/reservations/new">Create reservation</a>
        </div>
      } @else {
        <div class="reservations-list">
          @for (r of reservations(); track r.id) {
            <div class="reservation-card">
              <div class="reservation-card__header">
                <div>
                  <div class="reservation-card__time">
                    <tabler-icon [icon]="'calendar'" [size]="16"></tabler-icon>
                    {{ r.startTime | date:'medium' }}
                  </div>
                  <div class="reservation-card__meta">
                    <tabler-icon [icon]="'users'" [size]="14"></tabler-icon>
                    {{ r.numberOfGuests }} guests
                    @if (r.tableNumber) {
                      | Table {{ r.tableNumber }}
                    }
                  </div>
                  <div class="reservation-card__meta">
                    <tabler-icon [icon]="'building'" [size]="14"></tabler-icon>
                    {{ restaurantLabel(r.restaurantId) }}
                  </div>
                  <div class="reservation-card__meta">
                    <tabler-icon [icon]="'clock'" [size]="14"></tabler-icon>
                    {{ r.approximateDurationMinutes }} min
                  </div>
                </div>
                <app-status-badge [status]="r.status ?? ''" type="reservation"></app-status-badge>
              </div>
              @if (isUpcoming(r)) {
                <div class="reservation-card__actions">
                  <a [routerLink]="['/restaurants', r.restaurantId]" class="btn btn--ghost">View restaurant</a>
                  <button class="btn btn--danger-ghost" (click)="confirmCancel(r)" aria-label="Cancel reservation">
                    <tabler-icon [icon]="'x'" [size]="14"></tabler-icon> Cancel
                  </button>
                </div>
              } @else {
                <div class="reservation-card__actions">
                  <a [routerLink]="['/my/reservations', r.id, 'review']" class="btn btn--ghost">Leave a Review</a>
                  <a [routerLink]="['/reservations/new']" [queryParams]="{ restaurantId: r.restaurantId }" class="btn btn--ghost">Book again</a>
                </div>
              }
            </div>
          }
        </div>
      }

      <app-confirm-dialog
        [open]="cancelId() !== null"
        title="Cancel Reservation"
        message="Are you sure you want to cancel this reservation?"
        confirmLabel="Cancel Reservation"
        (confirmed)="doCancel()"
        (cancelled)="cancelId.set(null)">
      </app-confirm-dialog>
    </div>
  `,
  styles: [`
    .page-container { max-width: 800px; margin: 0 auto; padding: var(--space-xl) var(--space-lg); }
    .page-header { display: flex; justify-content: space-between; align-items: center; gap: var(--space-md); margin-bottom: var(--space-xl); }
    h2 { margin: 0; }
    .reservations-list { display: flex; flex-direction: column; gap: var(--space-md); }
    .reservation-card {
      background: var(--surface-800); border: 1px solid var(--border-default);
      border-radius: var(--radius-lg); padding: var(--space-lg);
    }
    .reservation-card__header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: var(--space-md); }
    .reservation-card__time { display: flex; align-items: center; gap: var(--space-xs); font-weight: 600; color: var(--text-primary); margin-bottom: var(--space-xs); }
    .reservation-card__meta { display: flex; align-items: center; gap: var(--space-xs); font-size: var(--text-sm); color: var(--text-secondary); }
    .reservation-card__actions { display: flex; gap: var(--space-sm); }
    .btn {
      display: inline-flex; align-items: center; gap: var(--space-xs);
      padding: var(--space-xs) var(--space-md); border-radius: var(--radius-md);
      font-size: var(--text-sm); font-weight: 500; cursor: pointer; text-decoration: none; border: none; background: transparent;
    }
    .btn--primary { background: var(--accent-primary); color: white; }
    .btn--primary:hover { background: var(--accent-primary-hover); text-decoration: none; }
    .btn--ghost { border: 1px solid var(--border-default); color: var(--text-secondary); &:hover { background: var(--surface-700); color: var(--text-primary); text-decoration: none; } }
    .btn--danger-ghost { border: 1px solid var(--state-error); color: var(--state-error); &:hover { background: rgba(220,53,69,0.1); } }
    .empty-state { text-align: center; padding: var(--space-2xl); color: var(--text-tertiary); h3, p { margin: var(--space-sm) 0; } }
    @media (max-width: 768px) {
      .page-header { flex-direction: column; align-items: stretch; }
      .reservation-card__header { flex-direction: column; gap: var(--space-sm); }
      .reservation-card__actions { flex-wrap: wrap; }
    }
  `],
})
export class MyReservationsComponent implements OnInit {
  private readonly service = inject(ReservationsService);
  private readonly restaurantsService = inject(RestaurantsService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly reservations = signal<ReservationResponse[]>([]);
  readonly restaurantsById = signal<Record<string, RestaurantBrief>>({});
  readonly cancelId = signal<string | null>(null);

  ngOnInit(): void {
    this.loading.set(true);
    this.loadRestaurants();
    this.service.getMy().subscribe({
      next: res => { this.reservations.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  restaurantLabel(restaurantId: string): string {
    const restaurant = this.restaurantsById()[restaurantId];
    if (!restaurant) {
      return `Restaurant ${restaurantId.slice(0, 8)}`;
    }

    const city = restaurant.city?.trim();
    const address = restaurant.address?.trim();
    if (city && address) return `${city} - ${address}`;
    return city || address || `Restaurant ${restaurant.id.slice(0, 8)}`;
  }

  isUpcoming(r: ReservationResponse): boolean {
    return new Date(r.startTime) > new Date() && r.status?.toLowerCase() !== 'cancelled';
  }

  confirmCancel(r: ReservationResponse): void {
    this.cancelId.set(r.id);
  }

  doCancel(): void {
    const id = this.cancelId();
    if (!id) return;
    this.service.cancel(id).subscribe({
      next: () => {
        this.reservations.update(list => list.filter(r => r.id !== id));
        this.notifications.success('Reservation cancelled.');
        this.cancelId.set(null);
      },
      error: () => {
        this.notifications.error('Could not cancel reservation.');
        this.cancelId.set(null);
      },
    });
  }

  private loadRestaurants(): void {
    this.restaurantsService.getAll().subscribe({
      next: restaurants => {
        const map = restaurants.reduce<Record<string, RestaurantBrief>>((acc, restaurant) => {
          acc[restaurant.id] = restaurant;
          return acc;
        }, {});
        this.restaurantsById.set(map);
      },
      error: () => {
        this.notifications.error('Could not load restaurant metadata.');
      },
    });
  }
}


